using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MediatR;
using Shared.DDD;

namespace Shared.Data.Interceptors
{
    public class SaveChangesInterceptor : Microsoft.EntityFrameworkCore.Diagnostics.SaveChangesInterceptor
    {
        private readonly ILogger<SaveChangesInterceptor> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeProvider _timeProvider;
        private readonly string _currentUser;
        private readonly List<IDomainEvent> _domainEvents = new();

        public SaveChangesInterceptor(
            ILogger<SaveChangesInterceptor> logger,
            IServiceProvider serviceProvider,
            TimeProvider? timeProvider = null)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _timeProvider = timeProvider ?? TimeProvider.System;
            _currentUser = Environment.UserName ?? "System";
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData, 
            InterceptionResult<int> result)
        {
            if (eventData.Context != null)
            {
                ProcessChanges(eventData.Context);
            }
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, 
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context != null)
            {
                ProcessChanges(eventData.Context);
            }
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override int SavedChanges(
            SaveChangesCompletedEventData eventData, 
            int result)
        {
            if (eventData.Context != null)
            {
                // Dispatch events after successful save (fire-and-forget for sync)
                _ = Task.Run(async () => await DispatchDomainEventsAsync());
                LogSaveCompleted(eventData.Context, result);
            }
            return base.SavedChanges(eventData, result);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData, 
            int result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context != null)
            {
                // Dispatch events after successful save
                await DispatchDomainEventsAsync();
                LogSaveCompleted(eventData.Context, result);
            }
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        public override void SaveChangesFailed(DbContextErrorEventData eventData)
        {
            if (eventData.Context != null)
            {
                _domainEvents.Clear(); // Clear events on failure
                _logger.LogError(eventData.Exception, "SaveChanges failed for {ContextName}",
                    eventData.Context.GetType().Name);
            }
            base.SaveChangesFailed(eventData);
        }

        public override Task SaveChangesFailedAsync(
            DbContextErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context != null)
            {
                _domainEvents.Clear(); // Clear events on failure
                _logger.LogError(eventData.Exception, "SaveChanges failed for {ContextName}",
                    eventData.Context.GetType().Name);
            }
            return base.SaveChangesFailedAsync(eventData, cancellationToken);
        }

        private void ProcessChanges(DbContext context)
        {
            var now = _timeProvider.GetUtcNow().DateTime;

            foreach (var entry in context.ChangeTracker.Entries())
            {
                ProcessAuditFields(entry, now);
                ProcessSoftDeletes(entry, now);
                CollectDomainEvents(entry); // Collect events during save preparation
            }

            _logger.LogDebug("Processing {EntityCount} entity changes in {ContextName}", 
                context.ChangeTracker.Entries().Count(), 
                context.GetType().Name);
        }

        private void ProcessAuditFields(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, DateTime now)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    SetPropertyIfExists(entry, "CreatedAt", now);
                    SetPropertyIfExists(entry, "CreatedBy", _currentUser);
                    break;
                case EntityState.Modified:
                    SetPropertyIfExists(entry, "LastModifiedAt", now);
                    SetPropertyIfExists(entry, "LastModifiedBy", _currentUser);
                    break;
            }
        }

        private void ProcessSoftDeletes(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, DateTime now)
        {
            if (entry.State == EntityState.Deleted)
            {
                var isDeletedProperty = entry.Properties.FirstOrDefault(p => 
                    p.Metadata.Name.Equals("IsDeleted", StringComparison.OrdinalIgnoreCase));

                if (isDeletedProperty != null)
                {
                    entry.State = EntityState.Modified;
                    isDeletedProperty.CurrentValue = true;
                    SetPropertyIfExists(entry, "DeletedAt", now);
                    SetPropertyIfExists(entry, "DeletedBy", _currentUser);
                }
            }
        }

        private void CollectDomainEvents(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            if (entry.Entity is IAggregate aggregate && aggregate.Events.Any())
            {
                _domainEvents.AddRange(aggregate.Events);
                aggregate.ClearDomainEvents(); // Clear from aggregate
                
                _logger.LogDebug("Collected {EventCount} domain events from {EntityType}",
                    aggregate.Events.Count,
                    entry.Entity.GetType().Name);
            }
        }

        private async Task DispatchDomainEventsAsync()
        {
            if (!_domainEvents.Any()) return;

            var mediator = _serviceProvider.GetService<IMediator>();
            if (mediator == null)
            {
                _logger.LogWarning("MediatR not registered. Cannot dispatch {EventCount} domain events", _domainEvents.Count);
                _domainEvents.Clear();
                return;
            }

            var eventCount = _domainEvents.Count;
            var tasks = _domainEvents.Select(async domainEvent =>
            {
                try
                {
                    await mediator.Publish(domainEvent);
                    _logger.LogDebug("Dispatched domain event: {EventType}", domainEvent.GetType().Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to dispatch domain event: {EventType}", domainEvent.GetType().Name);
                }
            });

            await Task.WhenAll(tasks);
            
            _logger.LogInformation("Dispatched {EventCount} domain events", eventCount);
            _domainEvents.Clear();
        }

        private void LogSaveCompleted(DbContext context, int result)
        {
            _logger.LogInformation("SaveChanges completed for {ContextName}. {RecordsAffected} records affected",
                context.GetType().Name, result);
        }

        private void SetPropertyIfExists(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string propertyName, object? value)
        {
            var property = entry.Properties.FirstOrDefault(p => 
                p.Metadata.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

            if (property != null)
            {
                property.CurrentValue = value;
            }
        }
    }
}
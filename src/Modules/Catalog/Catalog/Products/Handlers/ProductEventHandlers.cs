using MediatR;
using Catalog.Products.Events;
using Microsoft.Extensions.Logging;

namespace Catalog.Products.Handlers
{
    // Example event handlers demonstrating the benefits of the IDomainEvent pattern

    public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
    {
        private readonly ILogger<ProductCreatedEventHandler> _logger;

        public ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Product created: ID={ProductId}, Name={Name}, Price={Price}, EventId={EventId}, OccurredOn={OccurredOn}",
                notification.ProductId,
                notification.Name,
                notification.Price,
                notification.EventId,
                notification.OccurredOn);

            // Additional business logic:
            // - Send welcome email to product creator
            // - Update search index
            // - Notify inventory system
            // - Log to audit trail

            return Task.CompletedTask;
        }
    }

    public class ProductPriceChangedEventHandler : INotificationHandler<ProductPriceChangedEvent>
    {
        private readonly ILogger<ProductPriceChangedEventHandler> _logger;

        public ProductPriceChangedEventHandler(ILogger<ProductPriceChangedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Product price changed: ID={ProductId}, OldPrice={OldPrice}, NewPrice={NewPrice}, Reason={Reason}, EventId={EventId}",
                notification.ProductId,
                notification.OldPrice,
                notification.NewPrice,
                notification.Reason,
                notification.EventId);

            // Additional business logic:
            // - Notify customers on wishlist
            // - Update pricing analytics
            // - Trigger discount rules
            // - Update competitor analysis

            return Task.CompletedTask;
        }
    }

    // Cross-module event handler example (would be in a different module)
    public class ProductEventForAnalyticsHandler : 
        INotificationHandler<ProductCreatedEvent>,
        INotificationHandler<ProductPriceChangedEvent>
    {
        private readonly ILogger<ProductEventForAnalyticsHandler> _logger;

        public ProductEventForAnalyticsHandler(ILogger<ProductEventForAnalyticsHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
        {
            // Send event to analytics system
            _logger.LogInformation("Sending product creation data to analytics: {ProductId}", notification.ProductId);
            return Task.CompletedTask;
        }

        public Task Handle(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
        {
            // Send pricing data to analytics system
            _logger.LogInformation("Sending price change data to analytics: {ProductId}", notification.ProductId);
            return Task.CompletedTask;
        }
    }
}
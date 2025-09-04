using MediatR;
using Microsoft.Extensions.Logging;
using Catalog.Products.Events;
using Catalog.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Products.Handlers.DomainEvents
{
    /// <summary>
    /// Handles product lifecycle events for search index updates
    /// </summary>
    public class ProductSearchIndexHandler : 
        INotificationHandler<ProductCreatedEvent>,
        INotificationHandler<ProductPriceChangedEvent>,
        INotificationHandler<ProductCategoriesUpdatedEvent>,
        INotificationHandler<ProductDeletedEvent>,
        INotificationHandler<ProductRestoredEvent>
    {
        private readonly ILogger<ProductSearchIndexHandler> _logger;
        private readonly CatalogDbContext _dbContext;

        public ProductSearchIndexHandler(
            ILogger<ProductSearchIndexHandler> logger,
            CatalogDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating search index for new product: {ProductId} - {ProductName}", 
                notification.ProductId, notification.Name);

            try
            {
                // Simulate search index update
                await SimulateSearchIndexUpdate(notification.ProductId, "CREATE", cancellationToken);
                
                _logger.LogInformation("Search index updated successfully for product creation: {ProductId}", 
                    notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update search index for product creation: {ProductId}", 
                    notification.ProductId);
                // Note: We don't throw here as this is a side effect, not critical to the main operation
            }
        }

        public async Task Handle(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating search index for price change: {ProductId} - {OldPrice} to {NewPrice}", 
                notification.ProductId, notification.OldPrice, notification.NewPrice);

            try
            {
                await SimulateSearchIndexUpdate(notification.ProductId, "PRICE_UPDATE", cancellationToken);
                
                _logger.LogInformation("Search index updated for price change: {ProductId}", notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update search index for price change: {ProductId}", 
                    notification.ProductId);
            }
        }

        public async Task Handle(ProductCategoriesUpdatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating search index for category change: {ProductId}", notification.ProductId);

            try
            {
                await SimulateSearchIndexUpdate(notification.ProductId, "CATEGORY_UPDATE", cancellationToken);
                
                _logger.LogInformation("Search index updated for category change: {ProductId}", notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update search index for category change: {ProductId}", 
                    notification.ProductId);
            }
        }

        public async Task Handle(ProductDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Removing product from search index: {ProductId} - {ProductName}", 
                notification.ProductId, notification.ProductName);

            try
            {
                await SimulateSearchIndexUpdate(notification.ProductId, "DELETE", cancellationToken);
                
                _logger.LogInformation("Product removed from search index: {ProductId}", notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove product from search index: {ProductId}", 
                    notification.ProductId);
            }
        }

        public async Task Handle(ProductRestoredEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Restoring product in search index: {ProductId} - {ProductName}", 
                notification.ProductId, notification.ProductName);

            try
            {
                await SimulateSearchIndexUpdate(notification.ProductId, "RESTORE", cancellationToken);
                
                _logger.LogInformation("Product restored in search index: {ProductId}", notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restore product in search index: {ProductId}", 
                    notification.ProductId);
            }
        }

        private async Task SimulateSearchIndexUpdate(Guid productId, string operation, CancellationToken cancellationToken)
        {
            // Simulate calling an external search service (Elasticsearch, Azure Search, etc.)
            await Task.Delay(100, cancellationToken); // Simulate network call
            
            // In a real implementation, you would:
            // 1. Get the current product data
            // 2. Transform it to search document format
            // 3. Send to search service (Elasticsearch, Azure Search, etc.)
            // 4. Handle any errors or retries
            
            _logger.LogDebug("Search index operation '{Operation}' completed for product {ProductId}", 
                operation, productId);
        }
    }
}
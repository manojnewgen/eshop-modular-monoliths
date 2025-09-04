using MediatR;
using Microsoft.Extensions.Logging;
using Catalog.Products.Events;

namespace Catalog.Products.Handlers.DomainEvents
{
    /// <summary>
    /// Handles product events for customer notifications
    /// </summary>
    public class ProductNotificationHandler : 
        INotificationHandler<ProductCreatedEvent>,
        INotificationHandler<ProductPriceChangedEvent>,
        INotificationHandler<ProductDeletedEvent>,
        INotificationHandler<ProductRestoredEvent>
    {
        private readonly ILogger<ProductNotificationHandler> _logger;

        public ProductNotificationHandler(ILogger<ProductNotificationHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending new product notifications: {ProductId} - {ProductName}", 
                notification.ProductId, notification.Name);

            try
            {
                // Notify subscribers interested in these categories
                await NotifySubscribers("NEW_PRODUCT", notification.ProductId, notification.Categories, cancellationToken);

                // Send to marketing for promotional campaigns
                await NotifyMarketing("PRODUCT_LAUNCH", notification, cancellationToken);

                // Update product recommendation engines
                await UpdateRecommendationEngine("PRODUCT_ADDED", notification.ProductId, notification.Categories, cancellationToken);

                _logger.LogInformation("New product notifications sent: {ProductId}", notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send new product notifications: {ProductId}", notification.ProductId);
            }
        }

        public async Task Handle(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending price change notifications: {ProductId}", notification.ProductId);

            try
            {
                // Notify users with this product in wishlist
                await NotifyWishlistUsers(notification.ProductId, notification.OldPrice, notification.NewPrice, cancellationToken);

                // Send price drop alerts if price decreased
                if (notification.NewPrice < notification.OldPrice)
                {
                    await SendPriceDropAlerts(notification.ProductId, notification.OldPrice, notification.NewPrice, cancellationToken);
                }

                // Update price tracking systems
                await UpdatePriceTracking(notification, cancellationToken);

                _logger.LogInformation("Price change notifications sent: {ProductId}", notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send price change notifications: {ProductId}", notification.ProductId);
            }
        }

        public async Task Handle(ProductDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending product deletion notifications: {ProductId}", notification.ProductId);

            try
            {
                // Notify users with this product in cart/wishlist
                await NotifyUsersProductUnavailable(notification.ProductId, notification.ProductName, cancellationToken);

                // Remove from recommendation engines
                await UpdateRecommendationEngine("PRODUCT_REMOVED", notification.ProductId, null, cancellationToken);

                _logger.LogInformation("Product deletion notifications sent: {ProductId}", notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send product deletion notifications: {ProductId}", notification.ProductId);
            }
        }

        public async Task Handle(ProductRestoredEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending product restoration notifications: {ProductId}", notification.ProductId);

            try
            {
                // Notify users who were affected by deletion
                await NotifyUsersProductRestored(notification.ProductId, notification.ProductName, cancellationToken);

                // Re-add to recommendation engines
                await UpdateRecommendationEngine("PRODUCT_RESTORED", notification.ProductId, null, cancellationToken);

                _logger.LogInformation("Product restoration notifications sent: {ProductId}", notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send product restoration notifications: {ProductId}", notification.ProductId);
            }
        }

        private async Task NotifySubscribers(string eventType, Guid productId, List<string> categories, CancellationToken cancellationToken)
        {
            await Task.Delay(150, cancellationToken);
            _logger.LogDebug("Subscriber notifications sent for {EventType}: {ProductId} in categories [{Categories}]", 
                eventType, productId, string.Join(", ", categories));
        }

        private async Task NotifyMarketing(string eventType, ProductCreatedEvent productEvent, CancellationToken cancellationToken)
        {
            await Task.Delay(100, cancellationToken);
            _logger.LogDebug("Marketing notification sent for {EventType}: {ProductId}", eventType, productEvent.ProductId);
        }

        private async Task UpdateRecommendationEngine(string operation, Guid productId, List<string>? categories, CancellationToken cancellationToken)
        {
            await Task.Delay(200, cancellationToken);
            _logger.LogDebug("Recommendation engine updated: {Operation} for product {ProductId}", operation, productId);
        }

        private async Task NotifyWishlistUsers(Guid productId, decimal oldPrice, decimal newPrice, CancellationToken cancellationToken)
        {
            await Task.Delay(175, cancellationToken);
            _logger.LogDebug("Wishlist users notified of price change: {ProductId} from ${OldPrice} to ${NewPrice}", 
                productId, oldPrice, newPrice);
        }

        private async Task SendPriceDropAlerts(Guid productId, decimal oldPrice, decimal newPrice, CancellationToken cancellationToken)
        {
            await Task.Delay(125, cancellationToken);
            var discountPercentage = ((oldPrice - newPrice) / oldPrice) * 100;
            _logger.LogInformation("PRICE DROP ALERT: Product {ProductId} reduced by {DiscountPercentage:F1}%", 
                productId, discountPercentage);
        }

        private async Task UpdatePriceTracking(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
        {
            await Task.Delay(75, cancellationToken);
            _logger.LogDebug("Price tracking updated for product {ProductId}", notification.ProductId);
        }

        private async Task NotifyUsersProductUnavailable(Guid productId, string productName, CancellationToken cancellationToken)
        {
            await Task.Delay(100, cancellationToken);
            _logger.LogDebug("Users notified of product unavailability: {ProductName} ({ProductId})", productName, productId);
        }

        private async Task NotifyUsersProductRestored(Guid productId, string productName, CancellationToken cancellationToken)
        {
            await Task.Delay(100, cancellationToken);
            _logger.LogDebug("Users notified of product restoration: {ProductName} ({ProductId})", productName, productId);
        }
    }

    /// <summary>
    /// Handles integration events for external systems
    /// </summary>
    public class ProductIntegrationHandler : 
        INotificationHandler<ProductCreatedEvent>,
        INotificationHandler<ProductPriceChangedEvent>,
        INotificationHandler<ProductDeletedEvent>
    {
        private readonly ILogger<ProductIntegrationHandler> _logger;

        public ProductIntegrationHandler(ILogger<ProductIntegrationHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Publishing integration events for new product: {ProductId}", notification.ProductId);

            try
            {
                // Publish to message bus for other microservices/systems
                await PublishIntegrationEvent("ProductCreated", new
                {
                    ProductId = notification.ProductId,
                    Name = notification.Name,
                    Price = notification.Price,
                    Categories = notification.Categories,
                    EventId = notification.EventId,
                    OccurredOn = notification.OccurredOn
                }, cancellationToken);

                // Sync with external catalog systems
                await SyncWithExternalCatalogs("CREATE", notification.ProductId, cancellationToken);

                // Update partner feeds
                await UpdatePartnerFeeds("PRODUCT_ADDED", notification.ProductId, cancellationToken);

                _logger.LogInformation("Integration events published for new product: {ProductId}", notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish integration events for new product: {ProductId}", notification.ProductId);
            }
        }

        public async Task Handle(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Publishing integration events for price change: {ProductId}", notification.ProductId);

            try
            {
                await PublishIntegrationEvent("ProductPriceChanged", new
                {
                    ProductId = notification.ProductId,
                    OldPrice = notification.OldPrice,
                    NewPrice = notification.NewPrice,
                    Reason = notification.Reason,
                    EventId = notification.EventId,
                    OccurredOn = notification.OccurredOn
                }, cancellationToken);

                await SyncWithExternalCatalogs("PRICE_UPDATE", notification.ProductId, cancellationToken);
                await UpdatePartnerFeeds("PRICE_CHANGED", notification.ProductId, cancellationToken);

                _logger.LogInformation("Integration events published for price change: {ProductId}", notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish integration events for price change: {ProductId}", notification.ProductId);
            }
        }

        public async Task Handle(ProductDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Publishing integration events for product deletion: {ProductId}", notification.ProductId);

            try
            {
                await PublishIntegrationEvent("ProductDeleted", new
                {
                    ProductId = notification.ProductId,
                    ProductName = notification.ProductName,
                    EventId = notification.EventId,
                    OccurredOn = notification.OccurredOn
                }, cancellationToken);

                await SyncWithExternalCatalogs("DELETE", notification.ProductId, cancellationToken);
                await UpdatePartnerFeeds("PRODUCT_REMOVED", notification.ProductId, cancellationToken);

                _logger.LogInformation("Integration events published for product deletion: {ProductId}", notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish integration events for product deletion: {ProductId}", notification.ProductId);
            }
        }

        private async Task PublishIntegrationEvent(string eventType, object eventData, CancellationToken cancellationToken)
        {
            // Simulate publishing to message bus (RabbitMQ, Azure Service Bus, etc.)
            await Task.Delay(250, cancellationToken);
            _logger.LogDebug("Integration event published: {EventType}", eventType);
        }

        private async Task SyncWithExternalCatalogs(string operation, Guid productId, CancellationToken cancellationToken)
        {
            // Simulate syncing with external catalog systems
            await Task.Delay(300, cancellationToken);
            _logger.LogDebug("External catalog sync completed: {Operation} for product {ProductId}", operation, productId);
        }

        private async Task UpdatePartnerFeeds(string operation, Guid productId, CancellationToken cancellationToken)
        {
            // Simulate updating partner data feeds
            await Task.Delay(200, cancellationToken);
            _logger.LogDebug("Partner feeds updated: {Operation} for product {ProductId}", operation, productId);
        }
    }
}
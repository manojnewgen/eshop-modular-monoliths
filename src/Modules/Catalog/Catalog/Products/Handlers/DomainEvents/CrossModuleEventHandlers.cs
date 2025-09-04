using MediatR;
using Microsoft.Extensions.Logging;
using Catalog.Products.Events;

namespace Catalog.Products.Handlers.DomainEvents
{
    /// <summary>
    /// Cross-module handler that would typically be in the Basket module
    /// Demonstrates how domain events enable loose coupling between modules
    /// </summary>
    public class ProductEventForBasketHandler : 
        INotificationHandler<ProductCreatedEvent>,
        INotificationHandler<ProductPriceChangedEvent>,
        INotificationHandler<ProductDeletedEvent>
    {
        private readonly ILogger<ProductEventForBasketHandler> _logger;

        public ProductEventForBasketHandler(ILogger<ProductEventForBasketHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[BASKET MODULE] New product available for basket: {ProductId} - {ProductName}", 
                notification.ProductId, notification.Name);

            try
            {
                // In the actual Basket module, this would:
                // 1. Enable the product for cart operations
                // 2. Set up product validation rules
                // 3. Initialize pricing cache
                await SimulateBasketProductSetup(notification.ProductId, notification.Price, cancellationToken);

                _logger.LogInformation("[BASKET MODULE] Product setup completed for basket operations: {ProductId}", 
                    notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BASKET MODULE] Failed to setup product for basket: {ProductId}", 
                    notification.ProductId);
            }
        }

        public async Task Handle(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[BASKET MODULE] Updating product price in all baskets: {ProductId} from ${OldPrice} to ${NewPrice}", 
                notification.ProductId, notification.OldPrice, notification.NewPrice);

            try
            {
                // In the actual Basket module, this would:
                // 1. Update cached product prices
                // 2. Recalculate basket totals for all users with this product
                // 3. Notify users of price changes in their baskets
                await SimulateBasketPriceUpdate(notification.ProductId, notification.NewPrice, cancellationToken);

                _logger.LogInformation("[BASKET MODULE] Product price updated in all baskets: {ProductId}", 
                    notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BASKET MODULE] Failed to update product price in baskets: {ProductId}", 
                    notification.ProductId);
            }
        }

        public async Task Handle(ProductDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[BASKET MODULE] Removing deleted product from all baskets: {ProductId}", 
                notification.ProductId);

            try
            {
                // In the actual Basket module, this would:
                // 1. Remove product from all user baskets
                // 2. Notify users about removed items
                // 3. Recalculate basket totals
                // 4. Log removal events for audit
                await SimulateBasketProductRemoval(notification.ProductId, cancellationToken);

                _logger.LogInformation("[BASKET MODULE] Product removed from all baskets: {ProductId}", 
                    notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BASKET MODULE] Failed to remove product from baskets: {ProductId}", 
                    notification.ProductId);
            }
        }

        private async Task SimulateBasketProductSetup(Guid productId, decimal price, CancellationToken cancellationToken)
        {
            await Task.Delay(100, cancellationToken);
            _logger.LogDebug("[BASKET MODULE] Product setup simulation completed for {ProductId}", productId);
        }

        private async Task SimulateBasketPriceUpdate(Guid productId, decimal newPrice, CancellationToken cancellationToken)
        {
            await Task.Delay(150, cancellationToken);
            _logger.LogDebug("[BASKET MODULE] Price update simulation completed for {ProductId}: ${NewPrice}", 
                productId, newPrice);
        }

        private async Task SimulateBasketProductRemoval(Guid productId, CancellationToken cancellationToken)
        {
            await Task.Delay(125, cancellationToken);
            _logger.LogDebug("[BASKET MODULE] Product removal simulation completed for {ProductId}", productId);
        }
    }

    /// <summary>
    /// Cross-module handler that would typically be in the Ordering module
    /// Demonstrates how domain events enable loose coupling between modules
    /// </summary>
    public class ProductEventForOrderingHandler : 
        INotificationHandler<ProductCreatedEvent>,
        INotificationHandler<ProductDeletedEvent>
    {
        private readonly ILogger<ProductEventForOrderingHandler> _logger;

        public ProductEventForOrderingHandler(ILogger<ProductEventForOrderingHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[ORDERING MODULE] New product available for ordering: {ProductId} - {ProductName}", 
                notification.ProductId, notification.Name);

            try
            {
                // In the actual Ordering module, this would:
                // 1. Enable product for order line items
                // 2. Set up product validation for orders
                // 3. Initialize order fulfillment rules
                await SimulateOrderingProductSetup(notification.ProductId, notification.Categories, cancellationToken);

                _logger.LogInformation("[ORDERING MODULE] Product setup completed for ordering: {ProductId}", 
                    notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ORDERING MODULE] Failed to setup product for ordering: {ProductId}", 
                    notification.ProductId);
            }
        }

        public async Task Handle(ProductDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[ORDERING MODULE] Product no longer available for new orders: {ProductId}", 
                notification.ProductId);

            try
            {
                // In the actual Ordering module, this would:
                // 1. Prevent new orders for this product
                // 2. Handle pending orders with this product
                // 3. Update fulfillment rules
                // 4. Notify order management system
                await SimulateOrderingProductDisabling(notification.ProductId, cancellationToken);

                _logger.LogInformation("[ORDERING MODULE] Product disabled for new orders: {ProductId}", 
                    notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ORDERING MODULE] Failed to disable product for ordering: {ProductId}", 
                    notification.ProductId);
            }
        }

        private async Task SimulateOrderingProductSetup(Guid productId, List<string> categories, CancellationToken cancellationToken)
        {
            await Task.Delay(80, cancellationToken);
            _logger.LogDebug("[ORDERING MODULE] Product setup simulation completed for {ProductId} with categories [{Categories}]", 
                productId, string.Join(", ", categories));
        }

        private async Task SimulateOrderingProductDisabling(Guid productId, CancellationToken cancellationToken)
        {
            await Task.Delay(90, cancellationToken);
            _logger.LogDebug("[ORDERING MODULE] Product disabling simulation completed for {ProductId}", productId);
        }
    }

    /// <summary>
    /// Demonstrates error handling and retry logic in domain event handlers
    /// </summary>
    public class ProductEventWithRetryHandler : INotificationHandler<ProductCreatedEvent>
    {
        private readonly ILogger<ProductEventWithRetryHandler> _logger;
        private static int _attemptCount = 0;

        public ProductEventWithRetryHandler(ILogger<ProductEventWithRetryHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[RETRY DEMO] Processing product creation with retry logic: {ProductId}", 
                notification.ProductId);

            const int maxRetries = 3;
            var currentAttempt = Interlocked.Increment(ref _attemptCount);

            try
            {
                // Simulate a service that fails occasionally
                if (currentAttempt % 3 == 1) // Fail on every 3rd attempt for demo
                {
                    throw new InvalidOperationException("Simulated external service failure");
                }

                await SimulateExternalServiceCall(notification.ProductId, cancellationToken);

                _logger.LogInformation("[RETRY DEMO] Successfully processed on attempt {Attempt}: {ProductId}", 
                    currentAttempt, notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RETRY DEMO] Failed to process product creation on attempt {Attempt}: {ProductId}", 
                    currentAttempt, notification.ProductId);

                // In a real implementation, you might:
                // 1. Use Polly for retry policies
                // 2. Send to dead letter queue after max retries
                // 3. Implement exponential backoff
                // 4. Alert operations team

                if (currentAttempt < maxRetries)
                {
                    _logger.LogWarning("[RETRY DEMO] Will retry processing for {ProductId}", notification.ProductId);
                    // Note: MediatR doesn't have built-in retry, you'd need to implement this
                    // or use libraries like Polly
                }
                else
                {
                    _logger.LogError("[RETRY DEMO] Max retries exceeded for {ProductId}", notification.ProductId);
                }
            }
        }

        private async Task SimulateExternalServiceCall(Guid productId, CancellationToken cancellationToken)
        {
            await Task.Delay(200, cancellationToken);
            _logger.LogDebug("[RETRY DEMO] External service call completed for {ProductId}", productId);
        }
    }
}
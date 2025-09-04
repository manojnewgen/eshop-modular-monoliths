using MediatR;
using Microsoft.Extensions.Logging;
using Catalog.Products.Events;

namespace Catalog.Products.Handlers.DomainEvents
{
    /// <summary>
    /// Handles pricing events for analytics and reporting
    /// </summary>
    public class ProductPricingAnalyticsHandler : INotificationHandler<ProductPriceChangedEvent>
    {
        private readonly ILogger<ProductPricingAnalyticsHandler> _logger;

        public ProductPricingAnalyticsHandler(ILogger<ProductPricingAnalyticsHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing price change analytics for product: {ProductId}", notification.ProductId);

            try
            {
                // Calculate price change metrics
                var priceChangePercentage = CalculatePriceChangePercentage(notification.OldPrice, notification.NewPrice);
                var priceChangeType = DeterminePriceChangeType(notification.OldPrice, notification.NewPrice);

                // Send to analytics system
                await SendPricingAnalytics(new PricingAnalyticsData
                {
                    ProductId = notification.ProductId,
                    OldPrice = notification.OldPrice,
                    NewPrice = notification.NewPrice,
                    ChangePercentage = priceChangePercentage,
                    ChangeType = priceChangeType,
                    Reason = notification.Reason,
                    Timestamp = notification.OccurredOn,
                    EventId = notification.EventId
                }, cancellationToken);

                // Check for significant price changes
                if (Math.Abs(priceChangePercentage) > 20) // More than 20% change
                {
                    await SendSignificantPriceChangeAlert(notification, priceChangePercentage, cancellationToken);
                }

                _logger.LogInformation("Pricing analytics processed for product: {ProductId}, Change: {ChangePercentage}%", 
                    notification.ProductId, priceChangePercentage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process pricing analytics for product: {ProductId}", 
                    notification.ProductId);
            }
        }

        private decimal CalculatePriceChangePercentage(decimal oldPrice, decimal newPrice)
        {
            if (oldPrice == 0) return 0;
            return ((newPrice - oldPrice) / oldPrice) * 100;
        }

        private string DeterminePriceChangeType(decimal oldPrice, decimal newPrice)
        {
            if (newPrice > oldPrice) return "INCREASE";
            if (newPrice < oldPrice) return "DECREASE";
            return "NO_CHANGE";
        }

        private async Task SendPricingAnalytics(PricingAnalyticsData data, CancellationToken cancellationToken)
        {
            // Simulate sending to analytics service
            await Task.Delay(75, cancellationToken);
            
            _logger.LogDebug("Pricing analytics data sent: {ProductId} - {ChangeType} {ChangePercentage}%", 
                data.ProductId, data.ChangeType, data.ChangePercentage);
        }

        private async Task SendSignificantPriceChangeAlert(ProductPriceChangedEvent notification, decimal changePercentage, CancellationToken cancellationToken)
        {
            await Task.Delay(50, cancellationToken);
            
            _logger.LogWarning("SIGNIFICANT PRICE CHANGE: Product {ProductId} changed by {ChangePercentage}% - Reason: {Reason}", 
                notification.ProductId, changePercentage, notification.Reason);
        }

        private record PricingAnalyticsData
        {
            public Guid ProductId { get; init; }
            public decimal OldPrice { get; init; }
            public decimal NewPrice { get; init; }
            public decimal ChangePercentage { get; init; }
            public string ChangeType { get; init; } = string.Empty;
            public string Reason { get; init; } = string.Empty;
            public DateTime Timestamp { get; init; }
            public Guid EventId { get; init; }
        }
    }

    /// <summary>
    /// Handles product events for business intelligence and reporting
    /// </summary>
    public class ProductBusinessIntelligenceHandler : 
        INotificationHandler<ProductCreatedEvent>,
        INotificationHandler<ProductPriceChangedEvent>,
        INotificationHandler<ProductCategoriesUpdatedEvent>
    {
        private readonly ILogger<ProductBusinessIntelligenceHandler> _logger;

        public ProductBusinessIntelligenceHandler(ILogger<ProductBusinessIntelligenceHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recording product creation metrics: {ProductId}", notification.ProductId);

            try
            {
                var metrics = new ProductCreationMetrics
                {
                    ProductId = notification.ProductId,
                    Name = notification.Name,
                    Price = notification.Price,
                    Categories = notification.Categories,
                    CreatedAt = notification.OccurredOn,
                    EventId = notification.EventId
                };

                await SendToBusinessIntelligence("PRODUCT_CREATED", metrics, cancellationToken);

                _logger.LogInformation("Product creation metrics recorded: {ProductId}", notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record product creation metrics: {ProductId}", notification.ProductId);
            }
        }

        public async Task Handle(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recording price change metrics: {ProductId}", notification.ProductId);

            try
            {
                var metrics = new PriceChangeMetrics
                {
                    ProductId = notification.ProductId,
                    OldPrice = notification.OldPrice,
                    NewPrice = notification.NewPrice,
                    Reason = notification.Reason,
                    ChangedAt = notification.OccurredOn,
                    EventId = notification.EventId
                };

                await SendToBusinessIntelligence("PRICE_CHANGED", metrics, cancellationToken);

                _logger.LogInformation("Price change metrics recorded: {ProductId}", notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record price change metrics: {ProductId}", notification.ProductId);
            }
        }

        public async Task Handle(ProductCategoriesUpdatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recording category change metrics: {ProductId}", notification.ProductId);

            try
            {
                var metrics = new CategoryChangeMetrics
                {
                    ProductId = notification.ProductId,
                    OldCategories = notification.OldCategories,
                    NewCategories = notification.NewCategories,
                    ChangedAt = notification.OccurredOn,
                    EventId = notification.EventId
                };

                await SendToBusinessIntelligence("CATEGORIES_CHANGED", metrics, cancellationToken);

                _logger.LogInformation("Category change metrics recorded: {ProductId}", notification.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record category change metrics: {ProductId}", notification.ProductId);
            }
        }

        private async Task SendToBusinessIntelligence(string eventType, object data, CancellationToken cancellationToken)
        {
            // Simulate sending to BI system (Power BI, Tableau, etc.)
            await Task.Delay(100, cancellationToken);
            
            _logger.LogDebug("Business intelligence data sent: {EventType}", eventType);
        }

        private record ProductCreationMetrics
        {
            public Guid ProductId { get; init; }
            public string Name { get; init; } = string.Empty;
            public decimal Price { get; init; }
            public List<string> Categories { get; init; } = new();
            public DateTime CreatedAt { get; init; }
            public Guid EventId { get; init; }
        }

        private record PriceChangeMetrics
        {
            public Guid ProductId { get; init; }
            public decimal OldPrice { get; init; }
            public decimal NewPrice { get; init; }
            public string Reason { get; init; } = string.Empty;
            public DateTime ChangedAt { get; init; }
            public Guid EventId { get; init; }
        }

        private record CategoryChangeMetrics
        {
            public Guid ProductId { get; init; }
            public List<string> OldCategories { get; init; } = new();
            public List<string> NewCategories { get; init; } = new();
            public DateTime ChangedAt { get; init; }
            public Guid EventId { get; init; }
        }
    }
}
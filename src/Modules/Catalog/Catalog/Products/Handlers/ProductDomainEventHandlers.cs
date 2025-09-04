using MediatR;
using Microsoft.Extensions.Logging;
using Catalog.Products.Events;

namespace Catalog.Products.Handlers
{
    public class ProductEventHandlers : 
        INotificationHandler<ProductCreatedEvent>,
        INotificationHandler<ProductPriceChangedEvent>,
        INotificationHandler<ProductDeletedEvent>,
        INotificationHandler<ProductRestoredEvent>
    {
        private readonly ILogger<ProductEventHandlers> _logger;

        public ProductEventHandlers(ILogger<ProductEventHandlers> logger)
        {
            _logger = logger;
        }

        public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Product created: {ProductId} - {Name} at price {Price}",
                notification.ProductId, notification.Name, notification.Price);
            
            return Task.CompletedTask;
        }

        public Task Handle(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Product price changed: {ProductId} from {OldPrice} to {NewPrice}. Reason: {Reason}",
                notification.ProductId, notification.OldPrice, notification.NewPrice, notification.Reason);
                
            return Task.CompletedTask;
        }

        public Task Handle(ProductDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Product soft deleted: {ProductId} - {ProductName}",
                notification.ProductId, notification.ProductName);
                
            return Task.CompletedTask;
        }

        public Task Handle(ProductRestoredEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Product restored: {ProductId} - {ProductName}",
                notification.ProductId, notification.ProductName);
                
            return Task.CompletedTask;
        }
    }
}
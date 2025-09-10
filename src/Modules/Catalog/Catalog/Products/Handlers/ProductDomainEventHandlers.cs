using MediatR;
using Microsoft.Extensions.Logging;
using Catalog.Products.Events;


namespace Catalog.Products.Handlers
{
    public class ProductEventHandlers(ILogger<ProductEventHandlers> _logger, IBus bus) :
        INotificationHandler<ProductCreatedEvent>,
        INotificationHandler<ProductPriceChangedEvent>,
        INotificationHandler<ProductDeletedEvent>,
        INotificationHandler<ProductRestoredEvent>
    {



        public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Product created: {ProductId} - {Name} at price {Price}",
                notification.ProductId, notification.Name, notification.Price);

            return Task.CompletedTask;
        }


        public async Task Handle(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Product price changed: {ProductId} from {OldPrice} to {NewPrice}. Reason: {Reason}",
                notification.ProductId, notification.OldPrice, notification.NewPrice, notification.Reason);

            //TODO: Publish product price changed integration event to update basket price.
            var integrationEvent = new ProductPriceChangedIntegrationEvent
            {
                ProductId = notification.ProductId,
                Name = notification.Product.Name,
                Category = notification.Product.Categories.ToList(), // Explicit conversion
                Price = notification.Product.Price,
                Description = notification.Product.Description,
                ImageFile = notification.Product.ImageFile
                // Set other required properties if needed
            };

            await bus.Publish(integrationEvent, cancellationToken);

        }

        public Task Handle(ProductRestoredEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Product restored: {ProductId} - {ProductName}",
                notification.ProductId, notification.ProductName);

            return Task.CompletedTask;
        }

        public Task Handle(ProductDeletedEvent notification, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
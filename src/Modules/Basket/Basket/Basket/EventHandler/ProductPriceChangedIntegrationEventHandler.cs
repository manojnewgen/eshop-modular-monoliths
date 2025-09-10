using Basket.Basket.Features.UpdateItemInBasket;
using MassTransit;
using MassTransit.Util.Scanning;
using Shared.Messaging.Events;

namespace Basket.Basket.EventHandler
{
    public class ProductPriceChangedIntegrationEventHandler(ISender sender, ILogger<ProductPriceChangedIntegrationEventHandler> logger)
        : IConsumer<ProductPriceChangedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<ProductPriceChangedIntegrationEvent> context)
        {
            logger.LogInformation("Received ProductPriceChangedIntegrationEvent for ProductId: {ProductId}, New Price: {Price}",
                 context.Message.ProductId, context.Message.Price);


            // mediater new command to update product price in basket items

            var command = new UpdadateInItemPriceBasketCommand(context.Message.ProductId, context.Message.Price);

            var result = await sender.Send(command);
            if (result.Success)
            {
                logger.LogInformation("Successfully updated basket items with new price for ProductId: {ProductId}", context.Message.ProductId);
            }
            else
            {
                logger.LogError("Failed to update basket items with new price for ProductId: {ProductId}. Error: {Error}",
                    context.Message.ProductId, result);
            }

        }
    }
}

using Basket.Basket.DTOs;
using Basket.Basket.Modules;
using Basket.Data.Repositories;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Basket.Basket.Features.BulkUpdateBasket
{
    public record BulkUpdateBasketCommand(
        Guid BasketId,
        List<BulkUpdateItem> Items
    ) : ICommand<BulkUpdateBasketResult>;

    public record BulkUpdateItem(
        Guid ProductId,
        string Color,
        int NewQuantity  // 0 to remove item
    );

    public record BulkUpdateBasketResult(
        ShoppingCartDto UpdatedBasket,
        List<string> ProcessedActions
    );

    public class BulkUpdateBasketHandler(IBasketRepository basketRepository, IMappingService mappingService)
        : ICommandHandler<BulkUpdateBasketCommand, BulkUpdateBasketResult>
    {
        public async Task<BulkUpdateBasketResult> Handle(BulkUpdateBasketCommand command, CancellationToken cancellationToken)
        {
            var cart = await basketRepository.GetBasketByIdAsync(command.BasketId, false, cancellationToken);

            if (cart == null)
                throw new NotFoundException(nameof(ShoppingCart), command.BasketId);

            var processedActions = new List<string>();

            foreach (var updateItem in command.Items)
            {
                var existingItem = cart.Items.FirstOrDefault(x => 
                    x.ProductId == updateItem.ProductId && x.Color == updateItem.Color);

                if (existingItem == null && updateItem.NewQuantity > 0)
                {
                    // Item doesn't exist and we want to add it - but we need price and product name
                    processedActions.Add($"Skipped adding new item {updateItem.ProductId} ({updateItem.Color}) - missing price and product name");
                    continue;
                }

                if (existingItem != null)
                {
                    if (updateItem.NewQuantity <= 0)
                    {
                        // Remove item
                        cart.RemoveItem(updateItem.ProductId, updateItem.Color);
                        processedActions.Add($"Removed item {updateItem.ProductId} ({updateItem.Color})");
                    }
                    else
                    {
                        // Update quantity by removing and re-adding
                        var productName = existingItem.ProductName;
                        var price = existingItem.Price;
                        
                        cart.RemoveItem(updateItem.ProductId, updateItem.Color);
                        cart.AddItem(updateItem.ProductId, updateItem.NewQuantity, updateItem.Color, price, productName);
                        processedActions.Add($"Updated item {updateItem.ProductId} ({updateItem.Color}) to quantity {updateItem.NewQuantity}");
                    }
                }
            }

            // Replace this line:
            // await basketRepository.SaveChangesAsync(cancellationToken);

            // With this line:
            await basketRepository.SaveChangesAsync(null, cancellationToken);

            var updatedCartDto = mappingService.Map<ShoppingCart, ShoppingCartDto>(cart);
            return new BulkUpdateBasketResult(updatedCartDto, processedActions);
        }
    }
}
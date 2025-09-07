using Basket.Basket.DTOs;
using Basket.Basket.Modules;
using Basket.Data.Repositories;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Basket.Basket.Features.UpdateItemQuantity
{
    public record UpdateItemQuantityCommand(
        Guid BasketId,
        Guid ProductId,
        string Color,
        int NewQuantity
    ) : ICommand<UpdateItemQuantityResult>;

    public record UpdateItemQuantityResult(
        ShoppingCartDto UpdatedBasket
    );

    public class UpdateItemQuantityHandler(IBasketRepository basketRepository, IMappingService mappingService)
        : ICommandHandler<UpdateItemQuantityCommand, UpdateItemQuantityResult>
    {
        public async Task<UpdateItemQuantityResult> Handle(UpdateItemQuantityCommand command, CancellationToken cancellationToken)
        {
            var cart = await basketRepository.GetBasketByIdAsync(command.BasketId, false, cancellationToken);

            if (cart == null)
                throw new NotFoundException(nameof(ShoppingCart), command.BasketId);

            var item = cart.Items.FirstOrDefault(x => x.ProductId == command.ProductId && x.Color == command.Color);
            if (item == null)
                throw new NotFoundException($"Item with ProductId {command.ProductId} and Color {command.Color} not found in basket");

            if (command.NewQuantity <= 0)
            {
                // Remove item if quantity is 0 or negative
                cart.RemoveItem(command.ProductId, command.Color);
            }
            else
            {
                // Update the quantity by removing and re-adding with new quantity
                var productName = item.ProductName;
                var price = item.Price;
                
                cart.RemoveItem(command.ProductId, command.Color);
                cart.AddItem(command.ProductId, command.NewQuantity, command.Color, price, productName);
            }

            await basketRepository.SaveChangesAsync(null, cancellationToken);

            var updatedCartDto = mappingService.Map<ShoppingCart, ShoppingCartDto>(cart);
            return new UpdateItemQuantityResult(updatedCartDto);
        }
    }
}
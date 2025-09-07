using Basket.Basket.DTOs;
using Basket.Basket.Modules;
using Basket.Data.Repositories;
using Shared.Exceptions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Basket.Basket.Features.UpdateBasket
{
    public record UpdateBasketCommand(
        Guid BasketId,
        ShoppingCartDto ShoppingCartDto
    ) : ICommand<UpdateBasketResult>;

    public record UpdateBasketResult(
        ShoppingCartDto UpdatedBasket
    );

    public class UpdateBasketHandler(IBasketRepository basketRepository, IMappingService mappingService) 
        : ICommandHandler<UpdateBasketCommand, UpdateBasketResult>
    {
        public async Task<UpdateBasketResult> Handle(UpdateBasketCommand command, CancellationToken cancellationToken)
        {
            var cart = await basketRepository.GetBasketByIdAsync(command.BasketId, false, cancellationToken);

            if (cart == null)
                throw new NotFoundException(nameof(ShoppingCart), command.BasketId);

            // Since UserName has a private setter and domain model doesn't allow changing it,
            // we need to create a new basket if the username is different
            if (cart.UserName != command.ShoppingCartDto.UserName)
            {
                throw new BadRequestException($"Cannot change basket username from '{cart.UserName}' to '{command.ShoppingCartDto.UserName}'. Create a new basket instead.");
            }

            // Remove all existing items using domain method
            var existingItems = cart.Items.ToList(); // Create a copy to avoid collection modification during iteration
            foreach (var item in existingItems)
            {
                cart.RemoveItem(item.ProductId, item.Color);
            }

            // Add new items using domain method
            foreach (var item in command.ShoppingCartDto.Items)
            {
                cart.AddItem(
                    item.ProductId,
                    item.Quantity,
                    item.Color,
                    item.Price,
                    item.ProductName
                );
            }

            await basketRepository.SaveChangesAsync(null, cancellationToken);

            var updatedCartDto = mappingService.Map<ShoppingCart, ShoppingCartDto>(cart);
            return new UpdateBasketResult(updatedCartDto);
        }
    }
}

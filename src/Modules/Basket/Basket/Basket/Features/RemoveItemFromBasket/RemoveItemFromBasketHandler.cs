using Basket.Basket.DTOs;
using Basket.Basket.Modules;
using Basket.Data.Repositories;
using Shared.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Basket.Basket.Features.RemoveItemFromBasket
{
    public record RemoveItemFromBasketCommand(
        Guid BasketId,
        Guid ProductId,
        string Color
    ) : ICommand<RemoveItemFromBasketResult>;

    public record RemoveItemFromBasketResult(
        ShoppingCartDto UpdatedBasket
    );

    public class RemoveItemFromBasketHandler(IBasketRepository basketRepository, IMappingService mappingService)
        : ICommandHandler<RemoveItemFromBasketCommand, RemoveItemFromBasketResult>
    {
        public async Task<RemoveItemFromBasketResult> Handle(RemoveItemFromBasketCommand command, CancellationToken cancellationToken)
        {
            var cart = await basketRepository.GetBasketByIdAsync(command.BasketId, false, cancellationToken);

            if (cart == null)
                throw new NotFoundException(nameof(ShoppingCart), command.BasketId);

            cart.RemoveItem(command.ProductId, command.Color);

            await basketRepository.SaveChangesAsync(null, cancellationToken);

            var updatedCartDto = mappingService.Map<ShoppingCart, ShoppingCartDto>(cart);
            return new RemoveItemFromBasketResult(updatedCartDto);
        }
    }
}

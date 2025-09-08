using Basket.Basket.DTOs;
using Basket.Basket.Modules;
using Basket.Data.Repositories;
using Shared.Contracts.CQRS;
using Shared.Exceptions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Basket.Basket.Features.ClearBasket
{
    public record ClearBasketCommand(
        Guid BasketId
    ) : ICommand<ClearBasketResult>;

    public record ClearBasketResult(
        ShoppingCartDto ClearedBasket
    );

    public class ClearBasketHandler(IBasketRepository basketRepository, IMappingService mappingService)
        : ICommandHandler<ClearBasketCommand, ClearBasketResult>
    {
        public async Task<ClearBasketResult> Handle(ClearBasketCommand command, CancellationToken cancellationToken)
        {
            var cart = await basketRepository.GetBasketByIdAsync(command.BasketId, false, cancellationToken);

            if (cart == null)
                throw new NotFoundException(nameof(ShoppingCart), command.BasketId);

            // Remove all items using domain method
            var itemsToRemove = cart.Items.ToList();
            foreach (var item in itemsToRemove)
            {
                cart.RemoveItem(item.ProductId, item.Color);
            }

            await basketRepository.SaveChangesAsync(null, cancellationToken);

            var cartDto = mappingService.Map<ShoppingCart, ShoppingCartDto>(cart);
            return new ClearBasketResult(cartDto);
        }
    }
}
using Basket.Basket.DTOs;
using Basket.Data.Repositories;
using Shared.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Basket.Basket.Features.AddItemToBasket
{
    public record AddItemToBasketCommand(
        Guid BasketId,
        Guid ProductId,
        int Quantity,
        string Color,
        decimal Price,
        string ProductName
    ) : ICommand<AddItemToBasketResult>;

    public record AddItemToBasketResult(
        ShoppingCartDto UpdatedBasket
    );

    public class AddItemToBasketHandler(IBasketRepository basketRepository, IMappingService mappingService) 
        : ICommandHandler<AddItemToBasketCommand, AddItemToBasketResult>
    {
        public async Task<AddItemToBasketResult> Handle(AddItemToBasketCommand command, CancellationToken cancellationToken)
        {
            var cart = await basketRepository.GetBasketByIdAsync(command.BasketId, false, cancellationToken);

            if (cart == null)
                throw new NotFoundException(nameof(ShoppingCart), command.BasketId);

            cart.AddItem(
                command.ProductId,
                command.Quantity,
                command.Color,
                command.Price,
                command.ProductName
            );

            await basketRepository.SaveChangesAsync(cart.UserName, cancellationToken);

            var updatedCartDto = mappingService.Map<ShoppingCart, ShoppingCartDto>(cart);
            return new AddItemToBasketResult(updatedCartDto);
        }
    }
}

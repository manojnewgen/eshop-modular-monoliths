using Basket.Basket.DTOs;
using Basket.Data.Repositories;
using Catalog.Contracts.Products.Features.GetProductById;
using Catalog.Products.Dtos;
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

    public class AddItemToBasketHandler(IBasketRepository basketRepository, IMappingService mappingService, ISender sender) 
        : ICommandHandler<AddItemToBasketCommand, AddItemToBasketResult>
    {
        public async Task<AddItemToBasketResult> Handle(AddItemToBasketCommand command, CancellationToken cancellationToken)
        {
            var cart = await basketRepository.GetBasketByIdAsync(command.BasketId, false, cancellationToken);

            if (cart == null)
                throw new NotFoundException(nameof(ShoppingCart), command.BasketId);

            var result = await sender.Send(new GetProductByIdQuery(command.ProductId), cancellationToken);

            if (result?.ProductDto == null)
                throw new NotFoundException(nameof(ProductDto), command.ProductId);

            cart.AddItem(
                command.ProductId,
                command.Quantity,
                command.Color,
                result.ProductDto.Price, // Fix CS1503: pass decimal, not string
                result.ProductDto.Name   // Fix CS8602: null check above ensures safe dereference
            );

            await basketRepository.SaveChangesAsync(cart.UserName, cancellationToken);

            var updatedCartDto = mappingService.Map<ShoppingCart, ShoppingCartDto>(cart);
            return new AddItemToBasketResult(updatedCartDto);
        }
    }
}

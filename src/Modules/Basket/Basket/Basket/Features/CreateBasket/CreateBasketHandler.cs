using Basket.Basket.DTOs;
using Basket.Basket.Modules;
using Basket.Data.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Basket.Basket.Features.CreateBasket
{
    public record CreateBasketCommand(
       ShoppingCartDto ShoppingCartDto
        ) : ICommand<CreateBasketResult>;

    public record CreateBasketResult(Guid BasketId);
    
    public class CreateBasketHandler(IBasketRepository basketRepository, IMappingService mappingService) : ICommandHandler<CreateBasketCommand, CreateBasketResult>
    {
        public async Task<CreateBasketResult> Handle(CreateBasketCommand command, CancellationToken cancellationToken)
        {
            var shoppingCart = CreateBasket(command.ShoppingCartDto);
            
            await basketRepository.CreateBasketAsync(shoppingCart, cancellationToken);
            return new CreateBasketResult(shoppingCart.Id);
        }

        private ShoppingCart CreateBasket(ShoppingCartDto shoppingCartDto)
        {
            var newBasket = ShoppingCart.Create(Guid.NewGuid(), shoppingCartDto.UserName);
            shoppingCartDto.Items.ForEach(item =>
            {
                newBasket.AddItem(item.ProductId, item.Quantity, item.Color, item.Price, item.ProductName);
            });
            return newBasket;
        }
    }
}

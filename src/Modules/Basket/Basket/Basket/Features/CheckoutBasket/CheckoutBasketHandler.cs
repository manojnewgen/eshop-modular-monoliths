using Basket.Basket.DTOs;
using Basket.Basket.Modules;
using Basket.Data.Repositories;
using Shared.Exceptions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Basket.Basket.Features.CheckoutBasket
{
    public record CheckoutBasketCommand(
        Guid BasketId
    ) : ICommand<CheckoutBasketResult>;

    public record CheckoutBasketResult(
        ShoppingCartDto BasketForCheckout,
        decimal TotalAmount,
        int TotalItems
    );

    public class CheckoutBasketHandler(IBasketRepository basketRepository, IMappingService mappingService)
        : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
    {
        public async Task<CheckoutBasketResult> Handle(CheckoutBasketCommand command, CancellationToken cancellationToken)
        {
            var cart = await basketRepository.GetBasketByIdAsync(command.BasketId, true, cancellationToken);

            if (cart == null)
                throw new NotFoundException(nameof(ShoppingCart), command.BasketId);

            if (!cart.Items.Any())
                throw new BadRequestException("Cannot checkout an empty basket");

            // Calculate totals
            var totalAmount = cart.Items.Sum(item => item.Price * item.Quantity);
            var totalItems = cart.Items.Sum(item => item.Quantity);

            // Mark basket as checked out or perform other business logic here
            // For now, we'll just return the basket data for checkout processing

            var cartDto = mappingService.Map<ShoppingCart, ShoppingCartDto>(cart);
            return new CheckoutBasketResult(cartDto, totalAmount, totalItems);
        }
    }
}
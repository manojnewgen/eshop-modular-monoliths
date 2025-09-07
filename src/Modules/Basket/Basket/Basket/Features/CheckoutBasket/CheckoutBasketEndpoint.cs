using Carter;
using MediatR;
using Mapster;
using Basket.Basket.DTOs;

namespace Basket.Basket.Features.CheckoutBasket
{
    public record CheckoutBasketRequest(Guid BasketId) : ICommand<CheckoutBasketResult>;
    public record CheckoutBasketResponse(
        ShoppingCartDto BasketForCheckout,
        decimal TotalAmount,
        int TotalItems
    );

    public class CheckoutBasketEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/baskets/{basketId:guid}/checkout", async (Guid basketId, ISender sender) =>
            {
                var command = new CheckoutBasketRequest(basketId);
                var result = await sender.Send(command);
                var response = result.Adapt<CheckoutBasketResponse>();
                return Results.Ok(response);
            })
            .WithName("CheckoutBasket")
            .WithTags("Basket")
            .WithSummary("Prepare basket for checkout")
            .WithDescription("Validates and prepares a basket for checkout, returning total amount and item count");
        }
    }
}
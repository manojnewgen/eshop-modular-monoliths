using Carter;
using MediatR;
using Mapster;
using Basket.Basket.DTOs;

namespace Basket.Basket.Features.RemoveItemFromBasket
{
    public record RemoveItemFromBasketRequest(
        Guid BasketId,
        Guid ProductId,
        string Color
    ) : ICommand<RemoveItemFromBasketResult>;
    public record RemoveItemFromBasketResponse(ShoppingCartDto UpdatedBasket);

    public class RemoveItemFromBasketEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/baskets/{basketId:guid}/items", async (Guid basketId, RemoveItemFromBasketRequest request, ISender sender) =>
            {
                var command = request.Adapt<RemoveItemFromBasketCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<RemoveItemFromBasketResponse>();
                return Results.Ok(response);
            })
            .WithName("RemoveItemFromBasket")
            .WithTags("Basket")
            .WithSummary("Remove item from basket")
            .WithDescription("Removes a specific item from the basket");
        }
    }
}
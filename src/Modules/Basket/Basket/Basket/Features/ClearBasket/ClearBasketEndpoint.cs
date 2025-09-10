using Carter;
using MediatR;
using Mapster;
using Basket.Basket.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Basket.Basket.Features.ClearBasket
{
    public record ClearBasketRequest(Guid BasketId);
    public record ClearBasketResponse(ShoppingCartDto ClearedBasket);

    public class ClearBasketEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/baskets/{basketId:guid}/clear", async (Guid basketId, [FromServices] ISender sender) =>
            {
                var request = new ClearBasketRequest(basketId);
                var command = request.Adapt<ClearBasketCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<ClearBasketResponse>();
                return Results.Ok(response);
            })
            .WithName("ClearBasket")
            .WithTags("Basket")
            .WithSummary("Clear all items from basket")
            .WithDescription("Removes all items from the specified basket but keeps the basket itself");
        }
    }
}
using Carter;
using MediatR;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Basket.Basket.Features.DeleteBasket
{
    public record DeleteBasketRequest(Guid BasketId) : ICommand<DeleteBasketResult>;
    public record DeleteBasketResponse(bool IsDeleted);

    public class DeleteBasketEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/baskets/{basketId:guid}", async (Guid basketId, [FromServices] ISender sender) =>
            {
                var request = new DeleteBasketRequest(basketId);
                var command = request.Adapt<DeleteBasketCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<DeleteBasketResponse>();
                return Results.Ok(response);
            })
            .WithName("DeleteBasket")
            .WithTags("Basket")
            .WithSummary("Delete basket")
            .WithDescription("Permanently deletes the specified basket and all its items");
        }
    }
}
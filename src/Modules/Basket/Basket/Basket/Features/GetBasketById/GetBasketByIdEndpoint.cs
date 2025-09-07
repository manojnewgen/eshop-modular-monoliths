using Carter;
using MediatR;
using Mapster;
using Basket.Basket.DTOs;

namespace Basket.Basket.Features.GetBasketById
{
    public record GetBasketByIdRequest(Guid BasketId);
    public record GetBasketByIdResponse(ShoppingCartDto Basket);

    public class GetBasketByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/baskets/{basketId:guid}", async (Guid basketId, ISender sender) =>
            {
                var query = new GetBasketByIdRequest(basketId).Adapt<GetBasketByIdQuery>();
                var result = await sender.Send(query);
                var response = result.Adapt<GetBasketByIdResponse>();
                return Results.Ok(response);
            })
            .WithName("GetBasketById")
            .WithTags("Basket")
            .WithSummary("Get basket by ID")
            .WithDescription("Retrieves a specific basket by its unique identifier");
        }
    }
}
using Carter;
using MediatR;
using Mapster;
using Basket.Basket.DTOs;

namespace Basket.Basket.Features.UpdateBasket
{
    public record UpdateBasketRequest(Guid BasketId, ShoppingCartDto ShoppingCartDto) : ICommand<UpdateBasketResult>;
    public record UpdateBasketResponse(ShoppingCartDto UpdatedBasket);

    public class UpdateBasketEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/baskets/{basketId:guid}", async (Guid basketId, UpdateBasketRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdateBasketCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<UpdateBasketResponse>();
                return Results.Ok(response);
            })
            .WithName("UpdateBasket")
            .WithTags("Basket")
            .WithSummary("Update basket")
            .WithDescription("Updates the entire basket with new data");
        }
    }
}
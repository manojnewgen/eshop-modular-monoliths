using Carter;
using MediatR;
using Mapster;
using Basket.Basket.DTOs;

namespace Basket.Basket.Features.UpdateItemQuantity
{
    public record UpdateItemQuantityRequest(
        Guid BasketId,
        Guid ProductId,
        string Color,
        int NewQuantity
    ) : ICommand<UpdateItemQuantityResult>;
    public record UpdateItemQuantityResponse(ShoppingCartDto UpdatedBasket);

    public class UpdateItemQuantityEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/baskets/{basketId:guid}/items/quantity", async (Guid basketId, UpdateItemQuantityRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdateItemQuantityCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<UpdateItemQuantityResponse>();
                return Results.Ok(response);
            })
            .WithName("UpdateItemQuantity")
            .WithTags("Basket")
            .WithSummary("Update item quantity in basket")
            .WithDescription("Updates the quantity of a specific item in the basket. Set quantity to 0 to remove the item.");
        }
    }
}
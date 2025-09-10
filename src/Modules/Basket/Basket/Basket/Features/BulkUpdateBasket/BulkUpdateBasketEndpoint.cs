using Carter;
using MediatR;
using Mapster;
using Basket.Basket.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Basket.Basket.Features.BulkUpdateBasket
{
    public record BulkUpdateBasketRequest(
        Guid BasketId,
        List<BulkUpdateItem> Items
    ) : ICommand<BulkUpdateBasketResult>;
    public record BulkUpdateBasketResponse(
        ShoppingCartDto UpdatedBasket,
        List<string> ProcessedActions
    );

    public class BulkUpdateBasketEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/baskets/{basketId:guid}/bulk-update", async (Guid basketId, [FromBody] BulkUpdateBasketRequest request, [FromServices] ISender sender) =>
            {
                var command = request.Adapt<BulkUpdateBasketCommand>();
                command = command with { BasketId = basketId }; // Ensure basketId from route is used
                var result = await sender.Send(command);
                var response = result.Adapt<BulkUpdateBasketResponse>();
                return Results.Ok(response);
            })
            .WithName("BulkUpdateBasket")
            .WithTags("Basket")
            .WithSummary("Bulk update multiple items in basket")
            .WithDescription("Updates quantities for multiple items in a single operation. Set quantity to 0 to remove items.");
        }
    }
}
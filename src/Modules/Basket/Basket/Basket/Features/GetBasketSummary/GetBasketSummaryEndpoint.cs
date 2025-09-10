using Carter;
using MediatR;
using Mapster;
using Basket.Basket.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Basket.Basket.Features.GetBasketSummary
{
    public record GetBasketSummaryRequest(Guid BasketId) : IQuery<GetBasketSummaryResult>;
    public record GetBasketSummaryResponse(
        Guid BasketId,
        string UserName,
        int TotalItems,
        decimal TotalAmount,
        int UniqueProducts,
        DateTime LastModified
    );

    public class GetBasketSummaryEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/baskets/{basketId:guid}/summary", async (Guid basketId, [FromServices] ISender sender) =>
            {
                var request = new GetBasketSummaryRequest(basketId);
                var query = request.Adapt<GetBasketSummaryQuery>();
                var result = await sender.Send(query);
                var response = result.Adapt<GetBasketSummaryResponse>();
                return Results.Ok(response);
            })
            .WithName("GetBasketSummary")
            .WithTags("Basket")
            .WithSummary("Get basket summary statistics")
            .WithDescription("Returns summary information about a basket including totals and statistics");
        }
    }
}
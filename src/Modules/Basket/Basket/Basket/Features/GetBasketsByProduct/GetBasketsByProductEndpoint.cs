using Carter;
using MediatR;
using Mapster;
using Basket.Basket.DTOs;

namespace Basket.Basket.Features.GetBasketsByProduct
{
    public record GetBasketsByProductRequest(Guid ProductId) : IQuery<GetBasketsByProductResult>;
    public record GetBasketsByProductResponse(List<BasketItemInfo> BasketsContainingProduct);

    public class GetBasketsByProductEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/baskets/product/{productId:guid}", async (Guid productId, ISender sender) =>
            {
                var query = new GetBasketsByProductRequest(productId);
                var result = await sender.Send(query);
                var response = result.Adapt<GetBasketsByProductResponse>();
                return Results.Ok(response);
            })
            .WithName("GetBasketsByProduct")
            .WithTags("Basket")
            .WithSummary("Get baskets containing a specific product")
            .WithDescription("Finds all baskets that contain a specific product");
        }
    }
}
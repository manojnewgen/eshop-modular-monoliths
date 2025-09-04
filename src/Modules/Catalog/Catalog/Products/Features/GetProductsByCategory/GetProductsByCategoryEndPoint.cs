using Carter;
using Catalog.Products.Dtos;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Products.Features.GetProductsByCategory
{
    public record GetProductsByCategoryRequest(
        string Category,
        int PageNumber = 1,
        int PageSize = 20);

    public record GetProductsByCategoryResponse(
        string Category,
        List<ProductDto> Products,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages);

    public class GetProductsByCategoryEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products/category/{category}", async (
                string category,
                int pageNumber,
                int pageSize,
                ISender sender) =>
            {
                var query = new GetProductsByCategoryQuery(
                    category,
                    pageNumber == 0 ? 1 : pageNumber,
                    pageSize == 0 ? 20 : pageSize);

                var result = await sender.Send(query);
                var response = result.Adapt<GetProductsByCategoryResponse>();
                
                return Results.Ok(response);
            })
            .WithName("GetProductsByCategory")
            .Produces<GetProductsByCategoryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get products by category")
            .WithDescription("Get products filtered by a specific category with pagination");
        }
    }
}
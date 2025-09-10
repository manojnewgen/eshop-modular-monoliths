using Carter;
using Catalog.Products.Dtos;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Products.Features.GetProducts
{
    public record GetProductsRequest(
        string? SearchTerm = null,
        string? Category = null,
        decimal? MinPrice = null,
        decimal? MaxPrice = null,
        bool IncludeDeleted = false,
        int PageNumber = 1,
        int PageSize = 20);

    public record GetProductsResponse(
        List<ProductDto> Products,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages);

    public class GetProductsEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products", async (
                string? searchTerm,
                string? category,
                decimal? minPrice,
                decimal? maxPrice,
                bool includeDeleted,
                int pageNumber,
                int pageSize,
                [FromServices] ISender sender) =>
            {
                var query = new GetProductsQuery(
                    searchTerm,
                    category,
                    minPrice,
                    maxPrice,
                    includeDeleted,
                    pageNumber == 0 ? 1 : pageNumber,
                    pageSize == 0 ? 20 : pageSize);

                var result = await sender.Send(query);
                var response = result.Adapt<GetProductsResponse>();
                
                return Results.Ok(response);
            })
            .WithName("GetProducts")
            .Produces<GetProductsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get products with filtering and pagination")
            .WithDescription("Get products with optional filtering by search term, category, price range and pagination");
        }
    }
}
using Carter;
using Catalog.Products.Dtos;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Products.Features.SearchProducts
{
    public record SearchProductsRequest(
        string SearchTerm,
        List<string>? Categories = null,
        decimal? MinPrice = null,
        decimal? MaxPrice = null,
        bool? InStock = null,
        int PageNumber = 1,
        int PageSize = 20);

    public record SearchProductsResponse(
        string SearchTerm,
        List<ProductDto> Products,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages);

    public class SearchProductsEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products/search", async (
                string searchTerm,
                string? categories,
                decimal? minPrice,
                decimal? maxPrice,
                bool? inStock,
                int pageNumber,
                int pageSize,
                ISender sender) =>
            {
                var categoryList = !string.IsNullOrEmpty(categories) 
                    ? categories.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                    : null;

                var query = new SearchProductsQuery(
                    searchTerm,
                    categoryList,
                    minPrice,
                    maxPrice,
                    inStock,
                    pageNumber == 0 ? 1 : pageNumber,
                    pageSize == 0 ? 20 : pageSize);

                var result = await sender.Send(query);
                var response = result.Adapt<SearchProductsResponse>();
                
                return Results.Ok(response);
            })
            .WithName("SearchProducts")
            .Produces<SearchProductsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Search products")
            .WithDescription("Advanced product search with filters for categories, price range and stock availability");
        }
    }
}
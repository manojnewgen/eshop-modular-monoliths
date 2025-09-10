using Carter;
using Catalog.Products.Dtos;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Products.Features.GetProduct
{
    record GetProductRequest(Guid ProductId);

    record GetProductResponse(
        ProductDto? Product);
    
    public class GetProductEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/product/{productId:guid}", async (Guid productId, [FromServices] ISender sender) =>
            {
                var query = new GetProductQuery(productId);
                var result = await sender.Send(query);
                
                if (result == null)
                {
                    return Results.NotFound($"Product with ID {productId} not found");
                }
                
                var response = new GetProductResponse(result);
                return Results.Ok(response);
            })
            .WithName("GetProduct")
            .Produces<GetProductResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Get product by ID")
            .WithDescription("Get a single product by its ID");
        }
    }
}

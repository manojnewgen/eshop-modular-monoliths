using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Products.Features.RestoreProduct
{
    public record RestoreProductRequest(Guid ProductId);

    public record RestoreProductResponse(
        Guid ProductId,
        bool Success,
        string? Message = null);

    public class RestoreProductEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/products/{productId:guid}/restore", async (
                Guid productId,
                ISender sender) =>
            {
                var command = new RestoreProductCommand(productId);
                var result = await sender.Send(command);
                var response = result.Adapt<RestoreProductResponse>();
                
                if (!response.Success)
                {
                    if (response.Message?.Contains("not found") == true)
                    {
                        return Results.NotFound(response);
                    }
                    return Results.BadRequest(response);
                }
                
                return Results.Ok(response);
            })
            .WithName("RestoreProduct")
            .Produces<RestoreProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Restore product")
            .WithDescription("Restore a soft-deleted product");
        }
    }
}
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Products.Features.UpdateProductStock
{
    public record UpdateProductStockRequest(
        int NewQuantity);

    public record UpdateProductStockResponse(
        Guid ProductId,
        int OldQuantity,
        int NewQuantity,
        bool Success,
        string? Message = null);

    public class UpdateProductStockEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch("/products/{productId:guid}/stock", async (
                Guid productId,
                UpdateProductStockRequest request,
                ISender sender) =>
            {
                var command = new UpdateProductStockCommand(
                    productId,
                    request.NewQuantity);

                var result = await sender.Send(command);
                var response = result.Adapt<UpdateProductStockResponse>();
                
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
            .WithName("UpdateProductStock")
            .Produces<UpdateProductStockResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update product stock")
            .WithDescription("Update the stock quantity of a specific product");
        }
    }
}
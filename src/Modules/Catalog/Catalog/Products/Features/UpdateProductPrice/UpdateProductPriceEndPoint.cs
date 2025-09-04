using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Products.Features.UpdateProductPrice
{
    public record UpdateProductPriceRequest(
        decimal NewPrice,
        string? Reason = null);

    public record UpdateProductPriceResponse(
        Guid ProductId,
        decimal OldPrice,
        decimal NewPrice,
        bool Success,
        string? Message = null);

    public class UpdateProductPriceEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch("/products/{productId:guid}/price", async (
                Guid productId,
                UpdateProductPriceRequest request,
                ISender sender) =>
            {
                var command = new UpdateProductPriceCommand(
                    productId,
                    request.NewPrice,
                    request.Reason);

                var result = await sender.Send(command);
                var response = result.Adapt<UpdateProductPriceResponse>();
                
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
            .WithName("UpdateProductPrice")
            .Produces<UpdateProductPriceResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update product price")
            .WithDescription("Update the price of a specific product");
        }
    }
}
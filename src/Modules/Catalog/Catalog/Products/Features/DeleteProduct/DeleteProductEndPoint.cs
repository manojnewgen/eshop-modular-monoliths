using Carter;
using Catalog.Products.Dtos;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Products.Features.DeleteProduct
{
    record DeleteProductRequest(Guid ProductId);
    record DeleteProductResponse(Guid ProductId, bool Success, string? Message = null);

    public class DeleteProductEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("delete", async ([FromBody] DeleteProductRequest request, [FromServices] ISender sender) =>
            {
                var command = request.Adapt<DeleteProductCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<DeleteProductResponse>();
                return Results.Ok(response);
            })
            .WithName("Delete")
            .Produces<DeleteProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Delete product")
            .WithDescription("Delete Product");
        }
    }
}

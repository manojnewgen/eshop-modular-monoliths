using Carter;
using Catalog.Products.Dtos;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Products.Features.DeleteProduct
{
    record DeleteProductRequest(Guid ProductId);

    record DeleteProductResponse(Guid ProductId, bool Success, string? Message = null);
    public class DeleteProductEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("delete", async (DeleteProductRequest request, ISender sender) =>
            {
                var command = request.Adapt<DeleteProductCommand>();
                var result = sender.Send(command);
                var response = result.Adapt<DeleteProductResponse>();
                return response;

            })
            .WithName("Delete")
            .Produces<DeleteProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Delete product")
            .WithDescription("Delete Product");
        }
    }
}

using System;
using Carter;
using MediatR;
using Mapster;
using Basket.Basket.DTOs;

namespace Basket.Basket.Features.CreateBasket
{
    public record CreateBasketRequest(ShoppingCartDto ShoppingCartDto);
    public record CreateBasketResponse(Guid BasketId);
    
    public class CreateBasketEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/baskets", async (CreateBasketRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateBasketCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<CreateBasketResponse>();
                return Results.Created($"/baskets/{response.BasketId}", response);
            })
            .WithName("CreateBasket")
            .WithTags("Basket")
            .WithSummary("Create a new basket")
            .WithDescription("Creates a new shopping basket for a user");
        }
    }
}

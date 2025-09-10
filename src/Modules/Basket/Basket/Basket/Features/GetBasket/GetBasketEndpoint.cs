using Carter;
using MediatR;
using Mapster;
using Basket.Basket.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Basket.Basket.Features.GetBasket
{
    public record GetBasketRequest(string UserName);
    public record GetBasketResponse(ShoppingCartDto Basket);

    public class GetBasketEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/baskets/user/{userName}/current", async (string userName, [FromServices] ISender sender) =>
            {
                var request = new GetBasketRequest(userName);
                var query = request.Adapt<GetBasketQuery>();
                var result = await sender.Send(query);
                var response = result.Adapt<GetBasketResponse>();
                return Results.Ok(response);
            })
            .WithName("GetBasket")
            .WithTags("Basket")
            .WithSummary("Get current basket for user")
            .WithDescription("Retrieves the current active basket for a specific user");
        }
    }
}
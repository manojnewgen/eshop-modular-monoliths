using Carter;
using MediatR;
using Mapster;
using Basket.Basket.DTOs;

namespace Basket.Basket.Features.GetBasketsByUser
{
    public record GetBasketsByUserRequest(string UserName);
    public record GetBasketsByUserResponse(List<ShoppingCartDto> Baskets);

    public class GetBasketsByUserEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/baskets/user/{userName}", async (string userName, ISender sender) =>
            {
                var query = new GetBasketsByUserRequest(userName).Adapt<GetBasketsByUserQuery>();
                var result = await sender.Send(query);
                var response = result.Adapt<GetBasketsByUserResponse>();
                return Results.Ok(response);
            })
            .WithName("GetBasketsByUser")
            .WithTags("Basket")
            .WithSummary("Get all baskets for a user")
            .WithDescription("Retrieves all baskets associated with a specific username");
        }
    }
}
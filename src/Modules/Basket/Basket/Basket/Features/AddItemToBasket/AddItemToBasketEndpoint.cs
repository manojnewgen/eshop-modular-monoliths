using Carter;
using MediatR;
using Mapster;
using Basket.Basket.DTOs;

namespace Basket.Basket.Features.AddItemToBasket
{
    public record AddItemToBasketRequest(
        Guid BasketId,
        Guid ProductId,
        int Quantity,
        string Color,
        decimal Price,
        string ProductName
    ) : ICommand<AddItemToBasketResult>;
    public record AddItemToBasketResponse(ShoppingCartDto UpdatedBasket);

    public class AddItemToBasketEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/baskets/{basketId:guid}/items", async (Guid basketId, AddItemToBasketRequest request, ISender sender) =>
            {
                var command = request.Adapt<AddItemToBasketCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<AddItemToBasketResponse>();
                return Results.Ok(response);
            })
            .WithName("AddItemToBasket")
            .WithTags("Basket")
            .WithSummary("Add item to basket")
            .WithDescription("Adds a new item to the specified basket");
        }
    }
}
using Carter;
using MediatR;
using Mapster;
using Basket.Basket.DTOs;

namespace Basket.Basket.Features.ValidateBasket
{
    public record ValidateBasketRequest(Guid BasketId) : ICommand<ValidateBasketResult>;
    public record ValidateBasketResponse(
        bool IsValid,
        List<BasketValidationIssue> Issues,
        ShoppingCartDto BasketData
    );

    public class ValidateBasketEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/baskets/{basketId:guid}/validate", async (Guid basketId, ISender sender) =>
            {
                var command = new ValidateBasketRequest(basketId);
                var result = await sender.Send(command);
                var response = result.Adapt<ValidateBasketResponse>();
                return Results.Ok(response);
            })
            .WithName("ValidateBasket")
            .WithTags("Basket")
            .WithSummary("Validate basket contents")
            .WithDescription("Validates all items in the basket and returns any issues found");
        }
    }
}
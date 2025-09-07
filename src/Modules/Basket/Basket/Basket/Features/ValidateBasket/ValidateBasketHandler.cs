using Basket.Basket.DTOs;
using Basket.Basket.Modules;
using Basket.Data.Repositories;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Basket.Basket.Features.ValidateBasket
{
    public record ValidateBasketCommand(
        Guid BasketId
    ) : ICommand<ValidateBasketResult>;

    public record BasketValidationIssue(
        Guid ProductId,
        string Color,
        string IssueType,
        string Description
    );

    public record ValidateBasketResult(
        bool IsValid,
        List<BasketValidationIssue> Issues,
        ShoppingCartDto BasketData
    );

    public class ValidateBasketHandler(IBasketRepository basketRepository, IMappingService mappingService)
        : ICommandHandler<ValidateBasketCommand, ValidateBasketResult>
    {
        public async Task<ValidateBasketResult> Handle(ValidateBasketCommand command, CancellationToken cancellationToken)
        {
            var cart = await basketRepository.GetBasketByIdAsync(command.BasketId, true, cancellationToken);

            if (cart == null)
                throw new NotFoundException(nameof(ShoppingCart), command.BasketId);

            var issues = new List<BasketValidationIssue>();

            // Validate each item in the basket
            foreach (var item in cart.Items)
            {
                // Check for negative quantities
                if (item.Quantity <= 0)
                {
                    issues.Add(new BasketValidationIssue(
                        item.ProductId,
                        item.Color,
                        "InvalidQuantity",
                        $"Item has invalid quantity: {item.Quantity}"
                    ));
                }

                // Check for negative or zero prices
                if (item.Price <= 0)
                {
                    issues.Add(new BasketValidationIssue(
                        item.ProductId,
                        item.Color,
                        "InvalidPrice",
                        $"Item has invalid price: {item.Price}"
                    ));
                }

                // Check for empty product names
                if (string.IsNullOrWhiteSpace(item.ProductName))
                {
                    issues.Add(new BasketValidationIssue(
                        item.ProductId,
                        item.Color,
                        "MissingProductName",
                        "Item has no product name"
                    ));
                }

                // In a real scenario, you might also validate:
                // - Product still exists in catalog
                // - Price hasn't changed significantly
                // - Product is still available in the requested quantity
                // - Product variant (color) is still available
            }

            var cartDto = mappingService.Map<ShoppingCart, ShoppingCartDto>(cart);
            return new ValidateBasketResult(
                !issues.Any(),
                issues,
                cartDto
            );
        }
    }
}
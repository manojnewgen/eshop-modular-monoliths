using Basket.Basket.Modules;
using Basket.Data.Repositories;
using Shared.Exceptions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Basket.Basket.Features.GetBasketSummary
{
    public record GetBasketSummaryQuery(
        Guid BasketId
    ) : IQuery<GetBasketSummaryResult>;

    public record GetBasketSummaryResult(
        Guid BasketId,
        string UserName,
        int TotalItems,
        decimal TotalAmount,
        int UniqueProducts,
        DateTime LastModified
    );

    public class GetBasketSummaryHandler(IBasketRepository basketRepository)
        : IQueryHandler<GetBasketSummaryQuery, GetBasketSummaryResult>
    {
        public async Task<GetBasketSummaryResult> Handle(GetBasketSummaryQuery query, CancellationToken cancellationToken)
        {
            var cart = await basketRepository.GetBasketByIdAsync(query.BasketId, true, cancellationToken);

            if (cart == null)
                throw new NotFoundException(nameof(ShoppingCart), query.BasketId);

            var totalItems = cart.Items.Sum(item => item.Quantity);
            var totalAmount = cart.Items.Sum(item => item.Price * item.Quantity);
            var uniqueProducts = cart.Items.Select(item => item.ProductId).Distinct().Count();

            // Get the last modified date from the entity if it has one, otherwise use a default
            var lastModified = DateTime.UtcNow; // In a real scenario, you'd track this in your entity

            return new GetBasketSummaryResult(
                cart.Id,
                cart.UserName,
                totalItems,
                totalAmount,
                uniqueProducts,
                lastModified
            );
        }
    }
}
using Basket.Basket.DTOs;
using Basket.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Basket.Basket.Features.GetBasketsByProduct
{
    public record GetBasketsByProductQuery(
        Guid ProductId
    ) : IQuery<GetBasketsByProductResult>;

    public record GetBasketsByProductResult(
        List<BasketItemInfo> BasketsContainingProduct
    );

    public record BasketItemInfo(
        Guid BasketId,
        string UserName,
        int Quantity,
        string Color,
        decimal Price
    );

    public class GetBasketsByProductHandler(IBasketRepository basketRepository)
        : IQueryHandler<GetBasketsByProductQuery, GetBasketsByProductResult>
    {
        public async Task<GetBasketsByProductResult> Handle(GetBasketsByProductQuery query, CancellationToken cancellationToken)
        {
            var baskets = await basketRepository.GetBasketsContainingProductAsync(query.ProductId, cancellationToken);

            var basketItems = baskets
                .SelectMany(basket => basket.Items
                    .Where(item => item.ProductId == query.ProductId)
                    .Select(item => new BasketItemInfo(
                        basket.Id,
                        basket.UserName,
                        item.Quantity,
                        item.Color,
                        item.Price
                    )))
                .ToList();

            return new GetBasketsByProductResult(basketItems);
        }
    }
}
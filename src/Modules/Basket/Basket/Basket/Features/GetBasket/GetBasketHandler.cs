using Basket.Basket.DTOs;
using Basket.Basket.Modules;
using Basket.Data;
using Basket.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Basket.Basket.Features.GetBasket
{
    public record GetBasketQuery(
        string UserName
    ) : IQuery<GetBasketResult>;

    public record GetBasketResult(
        ShoppingCartDto Basket
    );

    public class GetBasketHandler(IBasketRepository basketRepository, IMappingService mappingService) 
        : IQueryHandler<GetBasketQuery, GetBasketResult>
    {
        public async Task<GetBasketResult> Handle(GetBasketQuery query, CancellationToken cancellationToken)
        {
            var basket = await basketRepository.GetBasketAsync(query.UserName, true, cancellationToken);
            var basketDto = mappingService.Map<ShoppingCart, ShoppingCartDto>(basket);
            return new GetBasketResult(basketDto);
        }
    }
}

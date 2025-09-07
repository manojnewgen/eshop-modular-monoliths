using Basket.Basket.DTOs;
using Basket.Basket.Modules;
using Basket.Data.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Basket.Basket.Features.GetBasketsByUser
{
    public record GetBasketsByUserQuery(
        string UserName
    ) : IQuery<GetBasketsByUserResult>;

    public record GetBasketsByUserResult(
        List<ShoppingCartDto> Baskets
    );

    public class GetBasketsByUserHandler(IBasketRepository basketRepository, IMappingService mappingService)
        : IQueryHandler<GetBasketsByUserQuery, GetBasketsByUserResult>
    {
        public async Task<GetBasketsByUserResult> Handle(GetBasketsByUserQuery query, CancellationToken cancellationToken)
        {
            var carts = await basketRepository.GetBasketsByUserAsync(query.UserName, cancellationToken);

            var cartDtos = carts.Select(cart => mappingService.Map<ShoppingCart, ShoppingCartDto>(cart)).ToList();
            return new GetBasketsByUserResult(cartDtos);
        }
    }
}
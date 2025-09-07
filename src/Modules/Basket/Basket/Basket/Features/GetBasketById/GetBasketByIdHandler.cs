using Basket.Basket.DTOs;
using Basket.Basket.Modules;
using Basket.Data.Repositories;
using Shared.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Basket.Basket.Features.GetBasketById
{
    public record GetBasketByIdQuery(
        Guid BasketId
    ) : IQuery<GetBasketByIdResult>;

    public record GetBasketByIdResult(
        ShoppingCartDto Basket
    );

    public class GetBasketByIdHandler(IBasketRepository basketRepository, IMappingService mappingService)
        : IQueryHandler<GetBasketByIdQuery, GetBasketByIdResult>
    {
        public async Task<GetBasketByIdResult> Handle(GetBasketByIdQuery query, CancellationToken cancellationToken)
        {
            var cart = await basketRepository.GetBasketByIdAsync(query.BasketId, true, cancellationToken);

            if (cart == null)
                throw new NotFoundException(nameof(ShoppingCart), query.BasketId);

            var cartDto = mappingService.Map<ShoppingCart, ShoppingCartDto>(cart);
            return new GetBasketByIdResult(cartDto);
        }
    }
}
using Basket.Basket.Modules;
using Basket.Data.Repositories;
using Shared.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Basket.Basket.Features.DeleteBasket
{
    public record DeleteBasketCommand(
        Guid BasketId
    ) : ICommand<DeleteBasketResult>;

    public record DeleteBasketResult(
        bool IsDeleted
    );

    public class DeleteBasketHandler(IBasketRepository basketRepository) : ICommandHandler<DeleteBasketCommand, DeleteBasketResult>
    {
        public async Task<DeleteBasketResult> Handle(DeleteBasketCommand command, CancellationToken cancellationToken)
        {
            var isDeleted = await basketRepository.DeleteBasketByIdAsync(command.BasketId, cancellationToken);
            return new DeleteBasketResult(isDeleted);
        }
    }
}

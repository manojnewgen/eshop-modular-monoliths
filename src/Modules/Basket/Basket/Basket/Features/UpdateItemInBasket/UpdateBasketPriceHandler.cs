using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basket.Basket.Features.UpdateItemInBasket
{
    public record UpdadateInItemPriceBasketCommand(Guid productId, decimal price)
        : ICommand<UpdadateInItemPriceBaskeResult>;

    public record UpdadateInItemPriceBaskeResult(bool Success);


    public class UpdateItemPriceInBasketCommandValidator
        : AbstractValidator<UpdadateInItemPriceBasketCommand>
    {
        public UpdateItemPriceInBasketCommandValidator()
        {
            RuleFor(x => x.productId).NotEmpty().WithMessage("ProductId is required.");
            RuleFor(x => x.price).GreaterThan(0).WithMessage("Price must be greater than zero.");
        }
    }

    public class UpdateBasketPriceHandler(BasketDbContext dbContext) : ICommandHandler<UpdadateInItemPriceBasketCommand, UpdadateInItemPriceBaskeResult>
    {
        public async Task<UpdadateInItemPriceBaskeResult> Handle(UpdadateInItemPriceBasketCommand command, CancellationToken cancellationToken)
        {
            // Find shopping cart items with matching product id
            var itemsToUpdate = await dbContext.CartItems
                .Where(x => x.ProductId == command.productId)
                .ToListAsync(cancellationToken);

            if(!itemsToUpdate.Any())
            {
                return new UpdadateInItemPriceBaskeResult(false);
            }

            // Iterate items and update price with incoming command price

            foreach (var item in itemsToUpdate)
            {
                item.UpdatePrice(command.price);
            }

            // Save changes to database
            await dbContext.SaveChangesAsync(cancellationToken);

            // Return success result
            return new UpdadateInItemPriceBaskeResult(true);
        }
    }
}

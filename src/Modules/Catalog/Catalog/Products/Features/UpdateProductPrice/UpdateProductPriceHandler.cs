using Catalog.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Shared.CQRS;

namespace Catalog.Products.Features.UpdateProductPrice
{
    /// <summary>
    /// Command to update product price specifically
    /// </summary>
    public record UpdateProductPriceCommand(Guid ProductId, decimal NewPrice, string? Reason = null) : ICommand<UpdateProductPriceResult>;

    /// <summary>
    /// Result of updating product price
    /// </summary>
    public record UpdateProductPriceResult(Guid ProductId, decimal OldPrice, decimal NewPrice, bool Success, string? Message = null);

    /// <summary>
    /// Handler for UpdateProductPriceCommand - updates product price using domain model
    /// </summary>
    public class UpdateProductPriceHandler : ICommandHandler<UpdateProductPriceCommand, UpdateProductPriceResult>
    {
        private readonly CatalogDbContext _dbContext;
        private readonly ILogger<UpdateProductPriceHandler> _logger;

        public UpdateProductPriceHandler(CatalogDbContext dbContext, ILogger<UpdateProductPriceHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<UpdateProductPriceResult> Handle(UpdateProductPriceCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating product price: {ProductId} to {NewPrice}", command.ProductId, command.NewPrice);

            try
            {
                // Find the existing product
                var existingProduct = await _dbContext.Products
                    .FirstOrDefaultAsync(p => p.Id == command.ProductId, cancellationToken);

                if (existingProduct == null)
                {
                    _logger.LogWarning("Product not found for price update: {ProductId}", command.ProductId);
                    return new UpdateProductPriceResult(command.ProductId, 0, command.NewPrice, false, "Product not found");
                }

                var oldPrice = existingProduct.Price;

                if (oldPrice == command.NewPrice)
                {
                    _logger.LogInformation("Product price unchanged: {ProductId} - {Price}", command.ProductId, oldPrice);
                    return new UpdateProductPriceResult(command.ProductId, oldPrice, command.NewPrice, true, "Price unchanged");
                }

                // Update price using domain method
                var reason = command.Reason ?? "Price update via API";
                existingProduct.UpdatePrice(command.NewPrice, reason);

                // Save changes - this will trigger your SaveChanges interceptor automatically!
                // ? Audit fields set automatically (LastModifiedAt, LastModifiedBy)
                // ? Domain events dispatched automatically (ProductPriceChangedEvent)
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Product price updated successfully: {ProductId} from {OldPrice} to {NewPrice}", 
                    command.ProductId, oldPrice, command.NewPrice);

                return new UpdateProductPriceResult(command.ProductId, oldPrice, command.NewPrice, true, "Price updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update product price: {ProductId}", command.ProductId);
                return new UpdateProductPriceResult(command.ProductId, 0, command.NewPrice, false, $"Price update failed: {ex.Message}");
            }
        }
    }
}
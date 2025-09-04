using Catalog.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Shared.CQRS;

namespace Catalog.Products.Features.RestoreProduct
{
    /// <summary>
    /// Command to restore a soft-deleted product
    /// </summary>
    public record RestoreProductCommand(Guid ProductId) : ICommand<RestoreProductResult>;

    /// <summary>
    /// Result of restoring a product
    /// </summary>
    public record RestoreProductResult(Guid ProductId, bool Success, string? Message = null);

    /// <summary>
    /// Handler for RestoreProductCommand - restores a soft-deleted product using domain model
    /// </summary>
    public class RestoreProductHandler : ICommandHandler<RestoreProductCommand, RestoreProductResult>
    {
        private readonly CatalogDbContext _dbContext;
        private readonly ILogger<RestoreProductHandler> _logger;

        public RestoreProductHandler(CatalogDbContext dbContext, ILogger<RestoreProductHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<RestoreProductResult> Handle(RestoreProductCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Restoring product: {ProductId}", command.ProductId);

            try
            {
                // Find the existing product including soft-deleted ones
                var existingProduct = await _dbContext.Products
                    .IgnoreQueryFilters() // Include soft-deleted products
                    .FirstOrDefaultAsync(p => p.Id == command.ProductId, cancellationToken);

                if (existingProduct == null)
                {
                    _logger.LogWarning("Product not found for restoration: {ProductId}", command.ProductId);
                    return new RestoreProductResult(command.ProductId, false, "Product not found");
                }

                if (!existingProduct.IsDeleted)
                {
                    _logger.LogWarning("Product is not deleted, cannot restore: {ProductId}", command.ProductId);
                    return new RestoreProductResult(command.ProductId, false, "Product is not deleted");
                }

                // Restore using domain method
                existingProduct.Restore();

                // Save changes - this will trigger your SaveChanges interceptor automatically!
                // ? Audit fields set automatically (LastModifiedAt, LastModifiedBy)
                // ? Domain events dispatched automatically (ProductRestoredEvent)
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Product restored successfully: {ProductId}", command.ProductId);

                return new RestoreProductResult(command.ProductId, true, "Product restored successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restore product: {ProductId}", command.ProductId);
                return new RestoreProductResult(command.ProductId, false, $"Restore failed: {ex.Message}");
            }
        }
    }
}
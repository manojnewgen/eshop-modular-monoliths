namespace Catalog.Products.Features.DeleteProduct
{
    /// <summary>
    /// Command to soft delete a product
    /// </summary>
    public record DeleteProductCommand(Guid ProductId) : ICommand<DeleteProductResult>;

    /// <summary>
    /// Result of deleting a product
    /// </summary>
    public record DeleteProductResult(Guid ProductId, bool Success, string? Message = null);

    /// <summary>
    /// Handler for DeleteProductCommand - soft deletes a product using domain model
    /// </summary>
    public class DeleteProductHandler : ICommandHandler<DeleteProductCommand, DeleteProductResult>
    {
        private readonly CatalogDbContext _dbContext;
        private readonly ILogger<DeleteProductHandler> _logger;

        public DeleteProductHandler(CatalogDbContext dbContext, ILogger<DeleteProductHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<DeleteProductResult> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting product: {ProductId}", command.ProductId);

            try
            {
                // Find the existing product
                var existingProduct = await _dbContext.Products
                    .FirstOrDefaultAsync(p => p.Id == command.ProductId, cancellationToken);

                if (existingProduct == null)
                {
                    _logger.LogWarning("Product not found for deletion: {ProductId}", command.ProductId);
                    return new DeleteProductResult(command.ProductId, false, "Product not found");
                }

                if (existingProduct.IsDeleted)
                {
                    _logger.LogWarning("Product already deleted: {ProductId}", command.ProductId);
                    return new DeleteProductResult(command.ProductId, false, "Product already deleted");
                }

                // Soft delete using domain method
                existingProduct.SoftDelete();

                // Save changes - this will trigger your SaveChanges interceptor automatically!
                // ? Soft delete fields set automatically (IsDeleted, DeletedAt, DeletedBy)
                // ? Domain events dispatched automatically (ProductDeletedEvent)
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Product soft deleted successfully: {ProductId}", command.ProductId);

                return new DeleteProductResult(command.ProductId, true, "Product deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete product: {ProductId}", command.ProductId);
                return new DeleteProductResult(command.ProductId, false, $"Delete failed: {ex.Message}");
            }
        }
    }
}
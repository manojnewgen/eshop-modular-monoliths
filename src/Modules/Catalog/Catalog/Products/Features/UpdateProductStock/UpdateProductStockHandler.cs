namespace Catalog.Products.Features.UpdateProductStock
{
    /// <summary>
    /// Command to update product stock quantity
    /// </summary>
    public record UpdateProductStockCommand(Guid ProductId, int NewQuantity) : ICommand<UpdateProductStockResult>;

    /// <summary>
    /// Result of updating product stock
    /// </summary>
    public record UpdateProductStockResult(Guid ProductId, int OldQuantity, int NewQuantity, bool Success, string? Message = null);

    /// <summary>
    /// Handler for UpdateProductStockCommand - updates product stock using domain model
    /// </summary>
    public class UpdateProductStockHandler : ICommandHandler<UpdateProductStockCommand, UpdateProductStockResult>
    {
        private readonly CatalogDbContext _dbContext;
        private readonly ILogger<UpdateProductStockHandler> _logger;

        public UpdateProductStockHandler(CatalogDbContext dbContext, ILogger<UpdateProductStockHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<UpdateProductStockResult> Handle(UpdateProductStockCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating product stock: {ProductId} to {NewQuantity}", command.ProductId, command.NewQuantity);

            try
            {
                // Find the existing product
                var existingProduct = await _dbContext.Products
                    .FirstOrDefaultAsync(p => p.Id == command.ProductId, cancellationToken);

                if (existingProduct == null)
                {
                    _logger.LogWarning("Product not found for stock update: {ProductId}", command.ProductId);
                    return new UpdateProductStockResult(command.ProductId, 0, command.NewQuantity, false, "Product not found");
                }

                var oldQuantity = existingProduct.StockQuantity;

                if (oldQuantity == command.NewQuantity)
                {
                    _logger.LogInformation("Product stock unchanged: {ProductId} - {Quantity}", command.ProductId, oldQuantity);
                    return new UpdateProductStockResult(command.ProductId, oldQuantity, command.NewQuantity, true, "Stock unchanged");
                }

                // Update stock using domain method
                existingProduct.UpdateStock(command.NewQuantity);

                // Save changes - this will trigger your SaveChanges interceptor automatically!
                // ? Audit fields set automatically (LastModifiedAt, LastModifiedBy)
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Product stock updated successfully: {ProductId} from {OldQuantity} to {NewQuantity}", 
                    command.ProductId, oldQuantity, command.NewQuantity);

                return new UpdateProductStockResult(command.ProductId, oldQuantity, command.NewQuantity, true, "Stock updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update product stock: {ProductId}", command.ProductId);
                return new UpdateProductStockResult(command.ProductId, 0, command.NewQuantity, false, $"Stock update failed: {ex.Message}");
            }
        }
    }
}
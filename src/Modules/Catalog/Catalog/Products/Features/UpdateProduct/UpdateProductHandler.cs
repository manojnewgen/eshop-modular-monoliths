using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalog.Products.Dtos;
using Catalog.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Shared.CQRS;
using Shared.Mapping;

namespace Catalog.Products.Features.UpdateProduct
{
    /// <summary>
    /// Command to update an existing product using ProductDto
    /// </summary>
    public record UpdateProductCommand(Guid ProductId, ProductDto Product) : ICommand<UpdateProductResult>;

    /// <summary>
    /// Result of updating a product
    /// </summary>
    public record UpdateProductResult(Guid ProductId, bool Success, string? Message = null);

    /// <summary>
    /// Handler for UpdateProductCommand - updates an existing product using domain model and Mapster
    /// </summary>
    public class UpdateProductHandler : ICommandHandler<UpdateProductCommand, UpdateProductResult>
    {
        private readonly CatalogDbContext _dbContext;
        private readonly ILogger<UpdateProductHandler> _logger;
        private readonly IMappingService _mappingService;

        public UpdateProductHandler(
            CatalogDbContext dbContext, 
            ILogger<UpdateProductHandler> logger,
            IMappingService mappingService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _mappingService = mappingService;
        }

        public async Task<UpdateProductResult> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating product: {ProductId} - {ProductName} using Mapster", command.ProductId, command.Product.Name);

            try
            {
                // Find the existing product
                var existingProduct = await _dbContext.Products
                    .FirstOrDefaultAsync(p => p.Id == command.ProductId, cancellationToken);

                if (existingProduct == null)
                {
                    _logger.LogWarning("Product not found: {ProductId}", command.ProductId);
                    return new UpdateProductResult(command.ProductId, false, "Product not found");
                }

                // Use Mapster to map ProductDto to existing Product entity
                // The mapping configuration handles domain method calls and business logic
                _mappingService.Map(command.Product, existingProduct);

                // Save changes - this will trigger your SaveChanges interceptor automatically!
                // ✅ Audit fields set automatically (LastModifiedAt, LastModifiedBy)
                // ✅ Domain events dispatched automatically (ProductPriceChangedEvent, etc.)
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Product updated successfully: {ProductId} using Mapster mapping", command.ProductId);

                return new UpdateProductResult(command.ProductId, true, "Product updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update product: {ProductId} - {ProductName}", command.ProductId, command.Product.Name);
                return new UpdateProductResult(command.ProductId, false, $"Update failed: {ex.Message}");
            }
        }
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MediatR;
using Catalog.Products.Features.CreateProduct;
using Catalog.Products.Features.UpdateProduct;
using Catalog.Products.Dtos;

namespace Catalog.Examples
{
    /// <summary>
    /// Comprehensive example showing Create and Update operations with ProductDto
    /// </summary>
    public class ProductCrudExample
    {
        public static async Task DemonstrateProductCrudOperations(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ProductCrudExample>>();

            logger.LogInformation("?? Starting Complete Product CRUD Demonstration");

            try
            {
                // === CREATE OPERATION ===
                logger.LogInformation("?? === CREATE OPERATION ===");

                var newProduct = new ProductDto(
                    Id: Guid.Empty,
                    Name: "Ultrabook Laptop",
                    Description: "Lightweight ultrabook for professionals",
                    Price: 1199.99m,
                    ImageFile: "ultrabook.jpg",
                    Categories: new List<string> { "Electronics", "Computers", "Ultrabooks" },
                    StockQuantity: 25
                );

                var createCommand = new CreateProductCommand(newProduct);
                var createResult = await mediator.Send(createCommand);

                logger.LogInformation("? Product created: {ProductId}", createResult.ProductId);

                // === UPDATE OPERATION ===
                logger.LogInformation("?? === UPDATE OPERATION ===");

                // Update price and add more categories
                var updatedProduct = newProduct with 
                { 
                    Price = 1099.99m, // Reduced price
                    Categories = new List<string> { "Electronics", "Computers", "Ultrabooks", "Business", "Portable" },
                    StockQuantity = 30,
                    Description = "Lightweight ultrabook for professionals - Now with extended warranty!"
                };

                var updateCommand = new UpdateProductCommand(createResult.ProductId, updatedProduct);
                var updateResult = await mediator.Send(updateCommand);

                if (updateResult.Success)
                {
                    logger.LogInformation("? Product updated successfully");
                }

                // === MULTIPLE UPDATES ===
                logger.LogInformation("?? === MULTIPLE UPDATES ===");

                // Update 1: Change stock
                var stockUpdate = updatedProduct with { StockQuantity = 15 };
                var stockUpdateCommand = new UpdateProductCommand(createResult.ProductId, stockUpdate);
                await mediator.Send(stockUpdateCommand);
                logger.LogInformation("? Stock updated to 15");

                // Update 2: Change price again
                var priceUpdate = stockUpdate with { Price = 999.99m };
                var priceUpdateCommand = new UpdateProductCommand(createResult.ProductId, priceUpdate);
                await mediator.Send(priceUpdateCommand);
                logger.LogInformation("? Price reduced to $999.99");

                // Update 3: Add premium category
                var categoryUpdate = priceUpdate with 
                { 
                    Categories = new List<string> { "Electronics", "Computers", "Ultrabooks", "Business", "Portable", "Premium" }
                };
                var categoryUpdateCommand = new UpdateProductCommand(createResult.ProductId, categoryUpdate);
                await mediator.Send(categoryUpdateCommand);
                logger.LogInformation("? Added 'Premium' category");

                // === VALIDATION TESTS ===
                logger.LogInformation("?? === VALIDATION TESTS ===");

                // Test updating non-existent product
                var invalidCommand = new UpdateProductCommand(Guid.NewGuid(), newProduct);
                var invalidResult = await mediator.Send(invalidCommand);
                
                if (!invalidResult.Success)
                {
                    logger.LogInformation("? Validation: Non-existent product update correctly rejected");
                }

                // === SUMMARY ===
                logger.LogInformation("");
                logger.LogInformation("?? === OPERATION SUMMARY ===");
                logger.LogInformation("   Product ID: {ProductId}", createResult.ProductId);
                logger.LogInformation("   Operations performed:");
                logger.LogInformation("     ? CREATE: Initial product creation");
                logger.LogInformation("     ? UPDATE: Bulk property update");
                logger.LogInformation("     ? UPDATE: Stock quantity change");
                logger.LogInformation("     ? UPDATE: Price reduction");
                logger.LogInformation("     ? UPDATE: Category addition");
                logger.LogInformation("     ? VALIDATION: Non-existent product rejection");

                logger.LogInformation("");
                logger.LogInformation("?? === INTERCEPTOR BENEFITS ===");
                logger.LogInformation("   All operations automatically included:");
                logger.LogInformation("     ? Audit fields (CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy)");
                logger.LogInformation("     ? Domain events (ProductCreatedEvent, ProductPriceChangedEvent, etc.)");
                logger.LogInformation("     ? Event dispatching via MediatR");
                logger.LogInformation("     ? Structured logging");
                logger.LogInformation("     ? Error handling and rollback");

                logger.LogInformation("?? Complete Product CRUD demonstration finished successfully!");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "? Error during CRUD demonstration: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}
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
    /// Example demonstrating how to use UpdateProductHandler with ProductDto
    /// </summary>
    public class UpdateProductWithDtoExample
    {
        public static async Task DemonstrateUpdateProductWithDto(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<UpdateProductWithDtoExample>>();

            logger.LogInformation("?? Starting ProductDto-based UpdateProduct Demonstration");

            try
            {
                // 1. First create a product to update
                var originalProductDto = new ProductDto(
                    Id: Guid.Empty,
                    Name: "Basic Laptop",
                    Description: "Entry-level laptop for office work",
                    Price: 699.99m,
                    ImageFile: "basic-laptop.jpg",
                    Categories: new List<string> { "Electronics", "Computers" },
                    StockQuantity: 20
                );

                logger.LogInformation("?? Creating original product: {ProductName}", originalProductDto.Name);

                var createCommand = new CreateProductCommand(originalProductDto);
                var createResult = await mediator.Send(createCommand);

                logger.LogInformation("? Original product created with ID: {ProductId}", createResult.ProductId);

                // 2. Now update the product with new information
                var updatedProductDto = new ProductDto(
                    Id: createResult.ProductId, // Same ID
                    Name: "Gaming Laptop Pro Max", // Updated name
                    Description: "High-end gaming laptop with advanced graphics and cooling", // Updated description
                    Price: 1899.99m, // Updated price
                    ImageFile: "gaming-laptop-pro-max.jpg", // Updated image
                    Categories: new List<string> { "Electronics", "Gaming", "Computers", "High-End", "Premium" }, // Updated categories
                    StockQuantity: 5 // Updated stock
                );

                logger.LogInformation("?? Updating product to: {ProductName} - ${Price}", 
                    updatedProductDto.Name, updatedProductDto.Price);

                // 3. Create update command with ProductDto
                var updateCommand = new UpdateProductCommand(createResult.ProductId, updatedProductDto);

                // 4. Send through MediatR
                var updateResult = await mediator.Send(updateCommand);

                if (updateResult.Success)
                {
                    logger.LogInformation("? Product updated successfully!");
                    logger.LogInformation("   Product ID: {ProductId}", updateResult.ProductId);
                    logger.LogInformation("   New Name: {ProductName}", updatedProductDto.Name);
                    logger.LogInformation("   New Price: ${Price}", updatedProductDto.Price);
                    logger.LogInformation("   New Categories: {Categories}", string.Join(", ", updatedProductDto.Categories));
                    logger.LogInformation("   New Stock: {StockQuantity}", updatedProductDto.StockQuantity);
                }
                else
                {
                    logger.LogError("? Product update failed: {Message}", updateResult.Message);
                }

                // 5. Try to update a non-existent product
                logger.LogInformation("?? Testing update of non-existent product...");

                var nonExistentProductDto = new ProductDto(
                    Id: Guid.Empty,
                    Name: "Non-existent Product",
                    Description: "This should fail",
                    Price: 99.99m,
                    ImageFile: "none.jpg",
                    Categories: new List<string> { "Test" },
                    StockQuantity: 1
                );

                var failCommand = new UpdateProductCommand(Guid.NewGuid(), nonExistentProductDto);
                var failResult = await mediator.Send(failCommand);

                if (!failResult.Success)
                {
                    logger.LogInformation("? Correctly handled non-existent product: {Message}", failResult.Message);
                }

                // 6. What happened behind the scenes:
                logger.LogInformation("");
                logger.LogInformation("?? What happened automatically during update:");
                logger.LogInformation("  ? MediatR routed command to UpdateProductHandler");
                logger.LogInformation("  ? Product found and validated");
                logger.LogInformation("  ? Domain methods called for each property change");
                logger.LogInformation("  ? Domain events raised (ProductPriceChangedEvent, ProductCategoriesUpdatedEvent)");
                logger.LogInformation("  ? SaveChanges interceptor set LastModifiedAt/LastModifiedBy");
                logger.LogInformation("  ? Domain events dispatched via MediatR");
                logger.LogInformation("  ? Result returned with success status");

                logger.LogInformation("?? ProductDto-based UpdateProduct demonstration completed!");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "? Error during UpdateProduct demonstration: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MediatR;
using Catalog.Products.Features.CreateProduct;
using Catalog.Products.Features.GetProduct;
using Catalog.Products.Features.GetProducts;
using Catalog.Products.Features.UpdateProduct;
using Catalog.Products.Features.DeleteProduct;
using Catalog.Products.Features.RestoreProduct;
using Catalog.Products.Features.UpdateProductPrice;
using Catalog.Products.Features.UpdateProductStock;
using Catalog.Products.Features.GetProductsByCategory;
using Catalog.Products.Features.SearchProducts;
using Catalog.Products.Dtos;

namespace Catalog.Examples
{
    /// <summary>
    /// Comprehensive example demonstrating all product handlers
    /// </summary>
    public class AllProductHandlersExample
    {
        public static async Task DemonstrateAllHandlers(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AllProductHandlersExample>>();

            logger.LogInformation("?? Starting Complete Product Handlers Demonstration");

            try
            {
                // ===== CREATE MULTIPLE PRODUCTS =====
                logger.LogInformation("?? === CREATING MULTIPLE PRODUCTS ===");

                var products = new List<(ProductDto dto, Guid id)>();

                // Create Gaming Laptop
                var gamingLaptop = new ProductDto(
                    Id: Guid.Empty,
                    Name: "Gaming Laptop RTX 4080",
                    Description: "High-end gaming laptop with RTX 4080",
                    Price: 2199.99m,
                    ImageFile: "gaming-laptop-rtx4080.jpg",
                    Categories: new List<string> { "Electronics", "Gaming", "Computers", "High-End" },
                    StockQuantity: 8
                );

                var createResult1 = await mediator.Send(new CreateProductCommand(gamingLaptop));
                products.Add((gamingLaptop, createResult1.ProductId));
                logger.LogInformation("? Created Gaming Laptop: {ProductId}", createResult1.ProductId);

                // Create Business Laptop
                var businessLaptop = new ProductDto(
                    Id: Guid.Empty,
                    Name: "Business Ultrabook",
                    Description: "Lightweight laptop for business professionals",
                    Price: 1299.99m,
                    ImageFile: "business-ultrabook.jpg",
                    Categories: new List<string> { "Electronics", "Computers", "Business" },
                    StockQuantity: 15
                );

                var createResult2 = await mediator.Send(new CreateProductCommand(businessLaptop));
                products.Add((businessLaptop, createResult2.ProductId));
                logger.LogInformation("? Created Business Laptop: {ProductId}", createResult2.ProductId);

                // Create Gaming Mouse
                var gamingMouse = new ProductDto(
                    Id: Guid.Empty,
                    Name: "RGB Gaming Mouse",
                    Description: "High precision gaming mouse with RGB lighting",
                    Price: 79.99m,
                    ImageFile: "rgb-gaming-mouse.jpg",
                    Categories: new List<string> { "Electronics", "Gaming", "Accessories" },
                    StockQuantity: 50
                );

                var createResult3 = await mediator.Send(new CreateProductCommand(gamingMouse));
                products.Add((gamingMouse, createResult3.ProductId));
                logger.LogInformation("? Created Gaming Mouse: {ProductId}", createResult3.ProductId);

                // ===== QUERY OPERATIONS =====
                logger.LogInformation("?? === QUERY OPERATIONS ===");

                // Get single product
                var singleProduct = await mediator.Send(new GetProductQuery(createResult1.ProductId));
                logger.LogInformation("? Retrieved single product: {ProductName}", singleProduct?.Name);

                // Get all products
                var allProducts = await mediator.Send(new GetProductsQuery());
                logger.LogInformation("? Retrieved all products: {Count} products found", allProducts.Products.Count);

                // Get products by category - Gaming
                var gamingProducts = await mediator.Send(new GetProductsByCategoryQuery("Gaming"));
                logger.LogInformation("? Retrieved Gaming products: {Count} products found", gamingProducts.Products.Count);

                // Get products by category - Electronics
                var electronicsProducts = await mediator.Send(new GetProductsByCategoryQuery("Electronics"));
                logger.LogInformation("? Retrieved Electronics products: {Count} products found", electronicsProducts.Products.Count);

                // Search products
                var searchResults = await mediator.Send(new SearchProductsQuery(
                    SearchTerm: "laptop",
                    Categories: new List<string> { "Gaming" },
                    MinPrice: 1000m,
                    MaxPrice: 3000m
                ));
                logger.LogInformation("? Search results for 'laptop' in Gaming category: {Count} products found", searchResults.Products.Count);

                // ===== UPDATE OPERATIONS =====
                logger.LogInformation("?? === UPDATE OPERATIONS ===");

                // Update product price
                var priceUpdateResult = await mediator.Send(new UpdateProductPriceCommand(
                    createResult1.ProductId, 
                    1999.99m, 
                    "Black Friday discount"));
                logger.LogInformation("? Updated price: {ProductId} from ${OldPrice} to ${NewPrice}", 
                    priceUpdateResult.ProductId, priceUpdateResult.OldPrice, priceUpdateResult.NewPrice);

                // Update product stock
                var stockUpdateResult = await mediator.Send(new UpdateProductStockCommand(
                    createResult2.ProductId, 
                    25));
                logger.LogInformation("? Updated stock: {ProductId} from {OldQuantity} to {NewQuantity}", 
                    stockUpdateResult.ProductId, stockUpdateResult.OldQuantity, stockUpdateResult.NewQuantity);

                // Full product update
                var updatedBusinessLaptop = businessLaptop with 
                { 
                    Name = "Premium Business Ultrabook Pro",
                    Description = "Enhanced business laptop with premium features",
                    Price = 1599.99m,
                    Categories = new List<string> { "Electronics", "Computers", "Business", "Premium" }
                };

                var fullUpdateResult = await mediator.Send(new UpdateProductCommand(createResult2.ProductId, updatedBusinessLaptop));
                logger.LogInformation("? Full product update: {ProductId} - {Success}", 
                    fullUpdateResult.ProductId, fullUpdateResult.Success);

                // ===== SOFT DELETE AND RESTORE =====
                logger.LogInformation("??? === SOFT DELETE AND RESTORE ===");

                // Soft delete a product
                var deleteResult = await mediator.Send(new DeleteProductCommand(createResult3.ProductId));
                logger.LogInformation("? Soft deleted product: {ProductId} - {Success}", 
                    deleteResult.ProductId, deleteResult.Success);

                // Verify product is not in normal queries
                var productsAfterDelete = await mediator.Send(new GetProductsQuery());
                logger.LogInformation("?? Products count after soft delete: {Count} (should be one less)", 
                    productsAfterDelete.Products.Count);

                // Get products including deleted
                var productsIncludingDeleted = await mediator.Send(new GetProductsQuery(IncludeDeleted: true));
                logger.LogInformation("?? Products count including deleted: {Count} (should include all)", 
                    productsIncludingDeleted.Products.Count);

                // Restore the deleted product
                var restoreResult = await mediator.Send(new RestoreProductCommand(createResult3.ProductId));
                logger.LogInformation("? Restored product: {ProductId} - {Success}", 
                    restoreResult.ProductId, restoreResult.Success);

                // Verify product is back in normal queries
                var productsAfterRestore = await mediator.Send(new GetProductsQuery());
                logger.LogInformation("?? Products count after restore: {Count} (should be back to original)", 
                    productsAfterRestore.Products.Count);

                // ===== ADVANCED FILTERING =====
                logger.LogInformation("?? === ADVANCED FILTERING ===");

                // Price range filtering
                var expensiveProducts = await mediator.Send(new GetProductsQuery(MinPrice: 1500m));
                logger.LogInformation("? Products over $1500: {Count} products", expensiveProducts.Products.Count);

                // Multiple category search
                var multiCategorySearch = await mediator.Send(new SearchProductsQuery(
                    SearchTerm: "",
                    Categories: new List<string> { "Gaming", "Business" }
                ));
                logger.LogInformation("? Products in Gaming or Business categories: {Count} products", 
                    multiCategorySearch.Products.Count);

                // In-stock filtering
                var inStockProducts = await mediator.Send(new SearchProductsQuery(
                    SearchTerm: "",
                    InStock: true
                ));
                logger.LogInformation("? In-stock products: {Count} products", inStockProducts.Products.Count);

                // ===== FINAL SUMMARY =====
                logger.LogInformation("");
                logger.LogInformation("?? === FINAL SUMMARY ===");
                logger.LogInformation("?? Handlers Demonstrated:");
                logger.LogInformation("   ? CreateProductHandler - Created {Count} products", products.Count);
                logger.LogInformation("   ? GetProductHandler - Retrieved single products");
                logger.LogInformation("   ? GetProductsHandler - Retrieved product lists with filtering");
                logger.LogInformation("   ? GetProductsByCategoryHandler - Retrieved products by category");
                logger.LogInformation("   ? SearchProductsHandler - Performed advanced searches");
                logger.LogInformation("   ? UpdateProductHandler - Performed full product updates");
                logger.LogInformation("   ? UpdateProductPriceHandler - Updated specific prices");
                logger.LogInformation("   ? UpdateProductStockHandler - Updated stock quantities");
                logger.LogInformation("   ? DeleteProductHandler - Performed soft deletes");
                logger.LogInformation("   ? RestoreProductHandler - Restored soft-deleted products");

                logger.LogInformation("");
                logger.LogInformation("?? === INTERCEPTOR BENEFITS (Applied to ALL handlers) ===");
                logger.LogInformation("   ? Automatic audit fields (CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy)");
                logger.LogInformation("   ? Domain events dispatched (ProductCreatedEvent, ProductPriceChangedEvent, etc.)");
                logger.LogInformation("   ? Soft delete conversion (hard deletes become soft deletes)");
                logger.LogInformation("   ? Structured logging for all operations");
                logger.LogInformation("   ? Error handling and transaction management");

                logger.LogInformation("?? Complete Product Handlers demonstration finished successfully!");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "? Error during handlers demonstration: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}
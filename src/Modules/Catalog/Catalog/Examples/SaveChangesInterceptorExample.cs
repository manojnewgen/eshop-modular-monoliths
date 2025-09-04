using Catalog.Data;
using Catalog.Products;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Examples
{
    public class SaveChangesInterceptorExample
    {
        private readonly ILogger<SaveChangesInterceptorExample> _logger;

        public SaveChangesInterceptorExample(ILogger<SaveChangesInterceptorExample> logger)
        {
            _logger = logger;
        }

        public async Task DemonstrateInterceptor(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

            _logger.LogInformation("Starting SaveChanges Interceptor Demonstration");

            try
            {
                // Create a new product
                var product = Product.Create(
                    id: Guid.NewGuid(),
                    name: "Demo Product",
                    description: "Demonstrates SaveChanges interceptor functionality",
                    price: 99.99m,
                    imageFile: "demo-product.jpg",
                    categories: new List<string> { "Demo", "Examples" },
                    stockQuantity: 10
                );

                context.Products.Add(product);
                var recordsAffected = await context.SaveChangesAsync();
                
                _logger.LogInformation("Product created. Records affected: {RecordsAffected}", recordsAffected);
                _logger.LogInformation("CreatedAt: {CreatedAt}, CreatedBy: {CreatedBy}", 
                    product.CreatedAt, product.CreatedBy);

                // Update the product
                product.UpdatePrice(149.99m, "Price increase demonstration");
                product.AddCategory("Updated");
                
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Product updated");
                _logger.LogInformation("LastModifiedAt: {LastModifiedAt}, LastModifiedBy: {LastModifiedBy}", 
                    product.LastModifiedAt, product.LastModifiedBy);

                // Soft delete the product
                product.SoftDelete();
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Product soft deleted. IsDeleted: {IsDeleted}, DeletedAt: {DeletedAt}", 
                    product.IsDeleted, product.DeletedAt);

                // Query excludes soft deleted items
                var products = context.Products.ToList();
                _logger.LogInformation("Active products found: {ProductCount}", products.Count);

                // Query including soft deleted items
                var allProducts = context.Products.IgnoreQueryFilters().ToList();
                _logger.LogInformation("All products (including deleted): {AllProductCount}", allProducts.Count);

                // Restore the product
                var productToRestore = context.Products.IgnoreQueryFilters()
                    .FirstOrDefault(p => p.Id == product.Id);
                
                if (productToRestore != null)
                {
                    productToRestore.Restore();
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Product restored. IsDeleted: {IsDeleted}", productToRestore.IsDeleted);
                }

                _logger.LogInformation("SaveChanges Interceptor Demonstration completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during demonstration: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task DemonstrateBulkOperations(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

            _logger.LogInformation("Starting Bulk Operations Demonstration");

            try
            {
                var products = new List<Product>();
                for (int i = 1; i <= 5; i++)
                {
                    var product = Product.Create(
                        id: Guid.NewGuid(),
                        name: $"Bulk Product {i}",
                        description: $"Bulk product number {i}",
                        price: 10.00m * i,
                        imageFile: $"bulk-product-{i}.jpg",
                        categories: new List<string> { "Bulk", $"Category{i}" },
                        stockQuantity: i * 5
                    );
                    products.Add(product);
                }

                context.Products.AddRange(products);
                var recordsAffected = await context.SaveChangesAsync();

                _logger.LogInformation("Bulk operation completed. {RecordsAffected} records affected", recordsAffected);

                foreach (var product in products)
                {
                    product.UpdatePrice(product.Price * 1.1m, "Bulk price increase");
                }

                await context.SaveChangesAsync();
                _logger.LogInformation("Bulk update completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk operations: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public static async Task RunDemonstration(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SaveChangesInterceptorExample>>();
            var example = new SaveChangesInterceptorExample(logger);
            
            await example.DemonstrateInterceptor(app);
            await example.DemonstrateBulkOperations(app);
        }
    }
}
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MediatR;
using Mapster;
using Shared.Mapping;
using Catalog.Products.Features.CreateProduct;
using Catalog.Products.Features.GetProduct;
using Catalog.Products.Features.UpdateProduct;
using Catalog.Products.Dtos;

namespace Catalog.Examples
{
    /// <summary>
    /// Example demonstrating Mapster integration with CQRS handlers
    /// </summary>
    public class MapsterIntegrationExample
    {
        public static async Task DemonstrateMapsterIntegration(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var mappingService = scope.ServiceProvider.GetRequiredService<IMappingService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<MapsterIntegrationExample>>();

            logger.LogInformation("?? Starting Mapster Integration Demonstration");

            try
            {
                // ===== DEMONSTRATE DIRECT MAPPING =====
                logger.LogInformation("?? === DIRECT MAPSTER MAPPING ===");

                // Create a ProductDto
                var productDto = new ProductDto(
                    Id: Guid.Empty,
                    Name: "Mapster Demo Laptop",
                    Description: "Laptop created to demonstrate Mapster mapping capabilities",
                    Price: 1499.99m,
                    ImageFile: "mapster-demo-laptop.jpg",
                    Categories: new List<string> { "Electronics", "Computers", "Demo" },
                    StockQuantity: 12
                );

                logger.LogInformation("?? Original ProductDto:");
                logger.LogInformation("   Name: {Name}", productDto.Name);
                logger.LogInformation("   Price: ${Price}", productDto.Price);
                logger.LogInformation("   Categories: {Categories}", string.Join(", ", productDto.Categories));

                // ===== DEMONSTRATE HANDLER WITH MAPSTER =====
                logger.LogInformation("?? === CQRS HANDLERS WITH MAPSTER ===");

                // Create product using handler (which uses Mapster internally)
                var createCommand = new CreateProductCommand(productDto);
                var createResult = await mediator.Send(createCommand);

                logger.LogInformation("? Product created using Mapster in CreateProductHandler");
                logger.LogInformation("   Created Product ID: {ProductId}", createResult.ProductId);

                // Get product using handler (which uses Mapster projection)
                var getQuery = new GetProductQuery(createResult.ProductId);
                var retrievedProduct = await mediator.Send(getQuery);

                if (retrievedProduct != null)
                {
                    logger.LogInformation("? Product retrieved using Mapster projection in GetProductHandler");
                    logger.LogInformation("   Retrieved Product: {Name} - ${Price}", retrievedProduct.Name, retrievedProduct.Price);
                }

                // Update product using handler (which uses Mapster for mapping)
                var updatedProductDto = productDto with 
                {
                    Name = "Updated Mapster Demo Laptop Pro",
                    Price = 1699.99m,
                    Categories = new List<string> { "Electronics", "Computers", "Demo", "Updated", "Premium" },
                    StockQuantity = 8
                };

                var updateCommand = new UpdateProductCommand(createResult.ProductId, updatedProductDto);
                var updateResult = await mediator.Send(updateCommand);

                if (updateResult.Success)
                {
                    logger.LogInformation("? Product updated using Mapster in UpdateProductHandler");
                    logger.LogInformation("   Update successful with smart domain method calls");
                }

                // ===== DEMONSTRATE MAPSTER FEATURES =====
                logger.LogInformation("? === MAPSTER ADVANCED FEATURES ===");

                // Show different mapping scenarios
                var anotherDto = new ProductDto(
                    Id: Guid.NewGuid(),
                    Name: "Another Product",
                    Description: "Another product for mapping demo",
                    Price: 999.99m,
                    ImageFile: "another.jpg",
                    Categories: new List<string> { "Test" },
                    StockQuantity: 5
                );

                // Generic mapping
                var mappedDto1 = mappingService.Map<ProductDto>(anotherDto);
                logger.LogInformation("? Generic mapping: {Name}", mappedDto1.Name);

                // Typed mapping
                var mappedDto2 = mappingService.Map<ProductDto, ProductDto>(anotherDto);
                logger.LogInformation("? Typed mapping: {Name}", mappedDto2.Name);

                // ===== PERFORMANCE BENEFITS =====
                logger.LogInformation("?? === MAPSTER PERFORMANCE BENEFITS ===");

                logger.LogInformation("?? Benefits demonstrated:");
                logger.LogInformation("   ? Database Projection - Mapster projects directly in SQL queries");
                logger.LogInformation("   ? No Manual Mapping - Automatic mapping based on conventions");
                logger.LogInformation("   ? Type Safety - Compile-time checking of mappings");
                logger.LogInformation("   ? Custom Logic - AfterMapping for domain method calls");
                logger.LogInformation("   ? Performance - Compiled expressions for fast execution");
                logger.LogInformation("   ? Memory Efficient - No intermediate objects created");

                // ===== CONFIGURATION BENEFITS =====
                logger.LogInformation("?? === MAPPING CONFIGURATION BENEFITS ===");

                logger.LogInformation("?? Configuration features used:");
                logger.LogInformation("   ? Custom Constructors - Uses Product.Create() factory method");
                logger.LogInformation("   ? Property Ignoring - Ignores audit and domain event fields");
                logger.LogInformation("   ? After Mapping Logic - Calls domain methods for business rules");
                logger.LogInformation("   ? Collection Handling - Smart category list management");
                logger.LogInformation("   ? Conditional Mapping - Only updates changed properties");

                // ===== COMPARISON =====
                logger.LogInformation("?? === BEFORE vs AFTER MAPSTER ===");

                logger.LogInformation("? Before Mapster:");
                logger.LogInformation("   - Manual Select() projections in every query handler");
                logger.LogInformation("   - Repetitive mapping code in create/update handlers");
                logger.LogInformation("   - Risk of mapping errors and inconsistencies");
                logger.LogInformation("   - Performance overhead from unnecessary data transfer");

                logger.LogInformation("? After Mapster:");
                logger.LogInformation("   - Automatic SQL projection with ProjectToType()");
                logger.LogInformation("   - Centralized, configured mapping logic");
                logger.LogInformation("   - Type-safe, compile-time checked mappings");
                logger.LogInformation("   - Optimal performance with minimal allocations");
                logger.LogInformation("   - Domain method integration for business logic");

                logger.LogInformation("?? Mapster Integration demonstration completed successfully!");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "? Error during Mapster demonstration: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}
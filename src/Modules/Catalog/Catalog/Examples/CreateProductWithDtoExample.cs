using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MediatR;
using Catalog.Products.Features.CreateProduct;
using Catalog.Products.Dtos;

namespace Catalog.Examples
{
    /// <summary>
    /// Example demonstrating how to use CreateProductHandler with ProductDto
    /// </summary>
    public class CreateProductWithDtoExample
    {
        public static async Task DemonstrateCreateProductWithDto(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<CreateProductWithDtoExample>>();

            logger.LogInformation("?? Starting ProductDto-based CreateProduct Demonstration");

            try
            {
                // 1. Create a ProductDto
                var productDto = new ProductDto(
                    Id: Guid.Empty, // This will be ignored, new ID generated
                    Name: "Gaming Laptop Pro",
                    Description: "High-performance gaming laptop with RTX graphics",
                    Price: 1599.99m,
                    ImageFile: "gaming-laptop-pro.jpg",
                    Categories: new List<string> { "Electronics", "Gaming", "Computers", "High-End" },
                    StockQuantity: 15
                );

                logger.LogInformation("?? Creating product: {ProductName} - ${Price}", 
                    productDto.Name, productDto.Price);

                // 2. Create command with ProductDto
                var command = new CreateProductCommand(productDto);

                // 3. Send through MediatR
                var result = await mediator.Send(command);

                logger.LogInformation("? Product created successfully!");
                logger.LogInformation("   Product ID: {ProductId}", result.ProductId);
                logger.LogInformation("   Product Name: {ProductName}", productDto.Name);
                logger.LogInformation("   Price: ${Price}", productDto.Price);
                logger.LogInformation("   Categories: {Categories}", string.Join(", ", productDto.Categories));

                // 4. What happened behind the scenes:
                logger.LogInformation("");
                logger.LogInformation("?? What happened automatically:");
                logger.LogInformation("  ? MediatR routed command to CreateProductHandler");
                logger.LogInformation("  ? ProductDto mapped to Product domain entity");
                logger.LogInformation("  ? Product.Create() called with validation");
                logger.LogInformation("  ? ProductCreatedEvent raised");
                logger.LogInformation("  ? SaveChanges interceptor set CreatedAt/CreatedBy");
                logger.LogInformation("  ? Domain events dispatched via MediatR");
                logger.LogInformation("  ? Result returned with new ProductId");

                logger.LogInformation("?? ProductDto-based CreateProduct demonstration completed!");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "? Error during ProductDto demonstration: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}
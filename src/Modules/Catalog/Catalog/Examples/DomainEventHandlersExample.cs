using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MediatR;
using Catalog.Products.Features.CreateProduct;
using Catalog.Products.Features.UpdateProduct;
using Catalog.Products.Features.UpdateProductPrice;
using Catalog.Products.Features.DeleteProduct;
using Catalog.Products.Features.RestoreProduct;
using Catalog.Products.Dtos;

namespace Catalog.Examples
{
    /// <summary>
    /// Comprehensive example demonstrating all domain event handlers in action
    /// </summary>
    public class DomainEventHandlersExample
    {
        public static async Task DemonstrateDomainEventHandlers(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<DomainEventHandlersExample>>();

            logger.LogInformation("?? Starting Comprehensive Domain Event Handlers Demonstration");

            try
            {
                // ===== PRODUCT CREATION EVENT =====
                logger.LogInformation("?? === PRODUCT CREATION EVENT ===");

                var newProduct = new ProductDto(
                    Id: Guid.Empty,
                    Name: "Premium Gaming Headset",
                    Description: "High-quality gaming headset with surround sound",
                    Price: 199.99m,
                    ImageFile: "gaming-headset.jpg",
                    Categories: new List<string> { "Electronics", "Gaming", "Audio" },
                    StockQuantity: 50
                );

                var createCommand = new CreateProductCommand(newProduct);
                var createResult = await mediator.Send(createCommand);

                logger.LogInformation("? Product created: {ProductId}", createResult.ProductId);
                logger.LogInformation("?? Watch the logs above to see all the domain event handlers responding to ProductCreatedEvent!");

                // Wait a moment to let all event handlers complete
                await Task.Delay(2000);

                // ===== PRICE CHANGE EVENT =====
                logger.LogInformation("?? === PRICE CHANGE EVENT ===");

                var priceCommand = new UpdateProductPriceCommand(createResult.ProductId, 179.99m, "Holiday discount");
                var priceResult = await mediator.Send(priceCommand);

                if (priceResult.Success)
                {
                    logger.LogInformation("? Price updated: {ProductId} from ${OldPrice} to ${NewPrice}", 
                        priceResult.ProductId, priceResult.OldPrice, priceResult.NewPrice);
                    logger.LogInformation("?? Watch the logs above to see all handlers responding to ProductPriceChangedEvent!");
                }

                await Task.Delay(2000);

                // ===== CATEGORY CHANGE EVENT =====
                logger.LogInformation("??? === CATEGORY CHANGE EVENT ===");

                var updatedProduct = newProduct with 
                { 
                    Categories = new List<string> { "Electronics", "Gaming", "Audio", "Premium", "Wireless" }
                };

                var updateCommand = new UpdateProductCommand(createResult.ProductId, updatedProduct);
                var updateResult = await mediator.Send(updateCommand);

                if (updateResult.Success)
                {
                    logger.LogInformation("? Categories updated: {ProductId}", updateResult.ProductId);
                    logger.LogInformation("?? Watch the logs above to see handlers responding to ProductCategoriesUpdatedEvent!");
                }

                await Task.Delay(2000);

                // ===== PRODUCT DELETION EVENT =====
                logger.LogInformation("??? === PRODUCT DELETION EVENT ===");

                var deleteCommand = new DeleteProductCommand(createResult.ProductId);
                var deleteResult = await mediator.Send(deleteCommand);

                if (deleteResult.Success)
                {
                    logger.LogInformation("? Product deleted (soft): {ProductId}", deleteResult.ProductId);
                    logger.LogInformation("?? Watch the logs above to see all handlers responding to ProductDeletedEvent!");
                }

                await Task.Delay(2000);

                // ===== PRODUCT RESTORATION EVENT =====
                logger.LogInformation("?? === PRODUCT RESTORATION EVENT ===");

                var restoreCommand = new RestoreProductCommand(createResult.ProductId);
                var restoreResult = await mediator.Send(restoreCommand);

                if (restoreResult.Success)
                {
                    logger.LogInformation("? Product restored: {ProductId}", restoreResult.ProductId);
                    logger.LogInformation("?? Watch the logs above to see all handlers responding to ProductRestoredEvent!");
                }

                await Task.Delay(2000);

                // ===== SUMMARY =====
                logger.LogInformation("");
                logger.LogInformation("?? === DOMAIN EVENT HANDLERS SUMMARY ===");
                logger.LogInformation("?? Event Handlers Demonstrated:");
                
                logger.LogInformation("?? SEARCH & INDEXING:");
                logger.LogInformation("   ? ProductSearchIndexHandler - Updates search indexes");
                logger.LogInformation("   ? Elasticsearch/Azure Search integration simulation");

                logger.LogInformation("?? INVENTORY MANAGEMENT:");
                logger.LogInformation("   ? ProductInventoryHandler - Inventory tracking lifecycle");
                logger.LogInformation("   ? ProductStockAlertHandler - Low stock alerts");

                logger.LogInformation("?? ANALYTICS & BUSINESS INTELLIGENCE:");
                logger.LogInformation("   ? ProductPricingAnalyticsHandler - Price change analytics");
                logger.LogInformation("   ? ProductBusinessIntelligenceHandler - BI metrics");

                logger.LogInformation("?? NOTIFICATIONS & COMMUNICATIONS:");
                logger.LogInformation("   ? ProductNotificationHandler - Customer notifications");
                logger.LogInformation("   ? Wishlist, marketing, and recommendation updates");

                logger.LogInformation("?? INTEGRATION & EXTERNAL SYSTEMS:");
                logger.LogInformation("   ? ProductIntegrationHandler - External system sync");
                logger.LogInformation("   ? Message bus publishing and partner feeds");

                logger.LogInformation("?? CROSS-MODULE COMMUNICATION:");
                logger.LogInformation("   ? ProductEventForBasketHandler - Basket module integration");
                logger.LogInformation("   ? ProductEventForOrderingHandler - Ordering module integration");
                logger.LogInformation("   ? ProductEventWithRetryHandler - Error handling demo");

                logger.LogInformation("");
                logger.LogInformation("?? === DOMAIN EVENT BENEFITS DEMONSTRATED ===");
                logger.LogInformation("? LOOSE COUPLING:");
                logger.LogInformation("   - Modules communicate without direct dependencies");
                logger.LogInformation("   - Easy to add new handlers without changing existing code");
                logger.LogInformation("   - Domain logic remains clean and focused");

                logger.LogInformation("? SCALABILITY:");
                logger.LogInformation("   - Multiple handlers can process the same event");
                logger.LogInformation("   - Handlers can be added or removed independently");
                logger.LogInformation("   - Easy to implement retry logic and error handling");

                logger.LogInformation("? AUDITABILITY:");
                logger.LogInformation("   - Every domain event has EventId and OccurredOn");
                logger.LogInformation("   - Complete audit trail of all business operations");
                logger.LogInformation("   - Easy to replay events for debugging");

                logger.LogInformation("? EVENTUAL CONSISTENCY:");
                logger.LogInformation("   - Side effects processed asynchronously");
                logger.LogInformation("   - System remains responsive during peak loads");
                logger.LogInformation("   - Graceful degradation if handlers fail");

                logger.LogInformation("? EXTENSIBILITY:");
                logger.LogInformation("   - New business requirements = new event handlers");
                logger.LogInformation("   - No changes to core domain logic required");
                logger.LogInformation("   - Easy to A/B test new features");

                logger.LogInformation("");
                logger.LogInformation("?? === TECHNICAL IMPLEMENTATION HIGHLIGHTS ===");
                logger.LogInformation("? MediatR INotification Pattern:");
                logger.LogInformation("   - IDomainEvent extends INotification");
                logger.LogInformation("   - Automatic handler discovery and execution");
                logger.LogInformation("   - Built-in dependency injection support");

                logger.LogInformation("?? SaveChanges Interceptor Integration:");
                logger.LogInformation("   - Events collected during entity changes");
                logger.LogInformation("   - Dispatched automatically after successful save");
                logger.LogInformation("   - Transactional consistency guaranteed");

                logger.LogInformation("??? Clean Architecture:");
                logger.LogInformation("   - Domain events in domain layer");
                logger.LogInformation("   - Event handlers in application layer");
                logger.LogInformation("   - Infrastructure concerns properly separated");

                logger.LogInformation("?? Domain Event Handlers demonstration completed successfully!");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "? Error during domain event handlers demonstration: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}
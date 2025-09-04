using Catalog.Products;
using Catalog.Products.Events;
using Catalog.Products.Exceptions;

namespace Catalog.Products.Examples
{
    /// <summary>
    /// Examples demonstrating how to use the Product rich domain entity
    /// </summary>
    public static class ProductUsageExamples
    {
        public static void CreateProductExample()
        {
            // Create a new product using the factory method
            var productId = Guid.NewGuid();
            var product = Product.Create(
                id: productId,
                name: "Wireless Headphones",
                description: "High-quality wireless headphones with noise cancellation",
                price: 299.99m,
                imageFile: "headphones.jpg",
                categories: new List<string> { "Electronics", "Audio", "Headphones" }
            );

            // The ProductCreatedEvent will be automatically raised
            var domainEvents = product.DomainEvents;
            // Process domain events...
        }

        public static void UpdateProductPriceExample()
        {
            // Assume we have an existing product
            var product = Product.Create(
                Guid.NewGuid(),
                "Gaming Mouse",
                "High-precision gaming mouse",
                79.99m,
                "mouse.jpg"
            );

            // Update price - this will raise ProductPriceChangedEvent
            product.UpdatePrice(69.99m, "Holiday sale discount");

            // Apply percentage discount
            product.ApplyDiscount(10m, "Customer loyalty discount");
        }

        public static void ManageCategoriesExample()
        {
            var product = Product.Create(
                Guid.NewGuid(),
                "Laptop",
                "Gaming laptop with RTX graphics",
                1299.99m,
                "laptop.jpg"
            );

            // Add single category
            product.AddCategory("Gaming");
            product.AddCategory("Computers");

            // Add multiple categories
            product.AddCategories(new[] { "High-Performance", "RTX" });

            // Remove category
            product.RemoveCategory("Gaming");

            // Check if product has category
            bool hasComputers = product.HasCategory("Computers");

            // Clear all categories
            product.ClearCategories();
        }

        public static void ValidationExample()
        {
            try
            {
                // This will throw InvalidProductNameException
                var product = Product.Create(
                    Guid.NewGuid(),
                    "", // Invalid name
                    "Description",
                    10.0m,
                    "image.jpg"
                );
            }
            catch (InvalidProductNameException ex)
            {
                // Handle validation error
                Console.WriteLine($"Validation error: {ex.Message}");
            }

            try
            {
                var product = Product.Create(
                    Guid.NewGuid(),
                    "Valid Product",
                    "Description",
                    10.0m,
                    "image.jpg"
                );

                // This will throw InvalidProductPriceException
                product.UpdatePrice(-5.0m);
            }
            catch (InvalidProductPriceException ex)
            {
                // Handle validation error
                Console.WriteLine($"Validation error: {ex.Message}");
            }
        }

        public static void BusinessLogicExample()
        {
            var product = Product.Create(
                Guid.NewGuid(),
                "Smartphone",
                "Latest smartphone model",
                899.99m,
                "phone.jpg",
                new List<string> { "Electronics", "Mobile", "Communication" }
            );

            // Check if product is in price range
            bool isAffordable = product.IsInPriceRange(500m, 1000m);

            // Update product details
            product.UpdateName("Smartphone Pro");
            product.UpdateDescription("Latest flagship smartphone model with advanced features");
            product.UpdateImageFile("phone_pro.jpg");

            // Domain events will be raised for price changes and category updates
            var events = product.DomainEvents;
            
            // Clear events after processing (typically done by infrastructure)
            var processedEvents = product.ClearDomainEvents();
        }
    }
}
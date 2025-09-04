using Catalog.Products.Events;
using Shared.DDD;

namespace Catalog.Products.Examples
{
    /// <summary>
    /// Simple demonstration you can run to see record behavior
    /// </summary>
    public static class QuickRecordTest
    {
        public static void RunRecordExamples()
        {
            Console.WriteLine("=== C# Record Functionality Demo ===\n");

            // Create an event
            var originalEvent = new ProductCreatedEvent(
                ProductId: Guid.Parse("12345678-1234-1234-1234-123456789012"),
                Name: "Gaming Mouse",
                Price: 79.99m,
                Categories: new List<string> { "Gaming", "Electronics", "Peripherals" }
            );

            Console.WriteLine("1. Original Event:");
            Console.WriteLine($"   {originalEvent}");
            Console.WriteLine($"   Event ID: {originalEvent.EventId}");
            Console.WriteLine($"   Occurred On: {originalEvent.OccurredOn}");
            Console.WriteLine();

            // Demonstrate 'with' expression
            var discountedEvent = originalEvent with { Price = 59.99m };
            Console.WriteLine("2. After applying discount (using 'with' expression):");
            Console.WriteLine($"   Original: {originalEvent.Name} - ${originalEvent.Price}");
            Console.WriteLine($"   Discounted: {discountedEvent.Name} - ${discountedEvent.Price}");
            Console.WriteLine($"   Same object? {ReferenceEquals(originalEvent, discountedEvent)}");
            Console.WriteLine();

            // Demonstrate equality
            var identicalEvent = new ProductCreatedEvent(
                originalEvent.ProductId,
                originalEvent.Name,
                originalEvent.Price,
                originalEvent.Categories
            );
            Console.WriteLine("3. Value Equality:");
            Console.WriteLine($"   Are events equal by content? {originalEvent == identicalEvent}");
            Console.WriteLine($"   Are they the same object reference? {ReferenceEquals(originalEvent, identicalEvent)}");
            Console.WriteLine();

            // Demonstrate deconstruction
            var (id, name, price, categories) = originalEvent;
            Console.WriteLine("4. Deconstruction:");
            Console.WriteLine($"   Extracted ID: {id}");
            Console.WriteLine($"   Extracted Name: {name}");
            Console.WriteLine($"   Extracted Price: {price}");
            Console.WriteLine($"   Extracted Categories: [{string.Join(", ", categories)}]");
            Console.WriteLine();

            // Demonstrate pattern matching
            var description = originalEvent switch
            {
                { Price: > 100 } => "Expensive product",
                { Price: > 50 } => "Mid-range product",
                { Categories: var cats } when cats.Contains("Gaming") => "Gaming product",
                _ => "Basic product"
            };
            Console.WriteLine("5. Pattern Matching:");
            Console.WriteLine($"   Product description: {description}");
            Console.WriteLine();

            // Show inheritance working
            Console.WriteLine("6. Inheritance from BaseDomainEvent:");
            Console.WriteLine($"   Event Type: {originalEvent.EventType}");
            Console.WriteLine($"   Can be treated as IDomainEvent: {originalEvent is IDomainEvent}");
            Console.WriteLine($"   Can be treated as BaseDomainEvent: {originalEvent is BaseDomainEvent}");
        }
    }
}
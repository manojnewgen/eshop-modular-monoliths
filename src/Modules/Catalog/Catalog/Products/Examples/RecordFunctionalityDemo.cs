using Catalog.Products.Events;
using Shared.DDD;

namespace Catalog.Products.Examples
{
    /// <summary>
    /// Demonstrates how C# records work with ProductCreatedEvent
    /// </summary>
    public static class RecordFunctionalityDemo
    {
        public static void DemonstrateRecordFeatures()
        {
            // 1. CREATING RECORDS
            var event1 = new ProductCreatedEvent(
                ProductId: Guid.NewGuid(),
                Name: "Laptop",
                Price: 999.99m,
                Categories: new List<string> { "Electronics", "Computers" }
            );

            // 2. IMMUTABILITY - Properties have 'init' setters
            // event1.Name = "New Name"; // ? This would cause a compile error!
            // event1.Price = 800m;      // ? This would cause a compile error!

            // 3. WITH EXPRESSIONS - Create new instances with modified values
            var event2 = event1 with 
            { 
                Price = 899.99m,  // Only change the price
                Name = "Gaming Laptop"  // And the name
            };
            // event1 remains unchanged, event2 is a new instance

            // 4. DECONSTRUCTION - Extract values easily
            var (productId, name, price, categories) = event1;
            Console.WriteLine($"Deconstructed: {productId}, {name}, {price}");

            // 5. VALUE EQUALITY - Compares by content, not reference
            var event3 = new ProductCreatedEvent(
                event1.ProductId,    // Same values
                event1.Name,
                event1.Price,
                event1.Categories
            );

            bool areEqual = event1 == event3;  // ? True! Same content
            bool areReferenceEqual = ReferenceEquals(event1, event3); // ? False! Different objects

            // 6. AUTOMATIC TOSTRING
            Console.WriteLine(event1.ToString());
            // Output: ProductCreatedEvent { ProductId = ..., Name = Laptop, Price = 999.99, Categories = ... }

            // 7. INHERITANCE - Gets properties from BaseDomainEvent
            Console.WriteLine($"Event ID: {event1.EventId}");
            Console.WriteLine($"Occurred On: {event1.OccurredOn}");
            Console.WriteLine($"Event Type: {event1.EventType}");

            // 8. PATTERN MATCHING
            var message = event1 switch
            {
                { Price: > 1000 } => "Expensive product created",
                { Categories.Count: > 3 } => "Product with many categories",
                { Name: var n } when n.Contains("Gaming") => "Gaming product created",
                _ => "Regular product created"
            };

            // 9. COLLECTIONS AND IMMUTABILITY CAVEAT
            // ?? Important: The List<string> itself is mutable!
            event1.Categories.Add("New Category"); // This modifies the original list!
            
            // Better approach for true immutability:
            var immutableEvent = new ProductCreatedEvent(
                Guid.NewGuid(),
                "Smartphone",
                699.99m,
                new List<string> { "Electronics", "Mobile" }.ToList() // Create a new list
            );
        }

        // 10. RECORD INHERITANCE EXAMPLE
        public static void DemonstrateInheritance()
        {
            var productEvent = new ProductCreatedEvent(
                Guid.NewGuid(),
                "Test Product",
                100m,
                new List<string>()
            );

            // Can be treated as base type
            BaseDomainEvent baseEvent = productEvent;
            IDomainEvent domainEvent = productEvent;

            // All base properties are available
            Console.WriteLine($"Base Event ID: {baseEvent.EventId}");
            Console.WriteLine($"Interface Event Type: {domainEvent.EventType}");
        }

        // 11. COMPARISON WITH TRADITIONAL CLASS
        public static void CompareWithTraditionalClass()
        {
            // Traditional class approach (what you'd need without records):
            /*
            public class ProductCreatedEventClass
            {
                public ProductCreatedEventClass(Guid productId, string name, decimal price, List<string> categories)
                {
                    ProductId = productId;
                    Name = name ?? throw new ArgumentNullException(nameof(name));
                    Price = price;
                    Categories = categories ?? throw new ArgumentNullException(nameof(categories));
                }

                public Guid ProductId { get; }
                public string Name { get; }
                public decimal Price { get; }
                public List<string> Categories { get; }

                public override bool Equals(object? obj)
                {
                    return obj is ProductCreatedEventClass other &&
                           ProductId.Equals(other.ProductId) &&
                           Name == other.Name &&
                           Price == other.Price &&
                           Categories.SequenceEqual(other.Categories);
                }

                public override int GetHashCode()
                {
                    return HashCode.Combine(ProductId, Name, Price, Categories);
                }

                public override string ToString()
                {
                    return $"ProductCreatedEventClass {{ ProductId = {ProductId}, Name = {Name}, Price = {Price}, Categories = [{string.Join(", ", Categories)}] }}";
                }

                public void Deconstruct(out Guid productId, out string name, out decimal price, out List<string> categories)
                {
                    productId = ProductId;
                    name = Name;
                    price = Price;
                    categories = Categories;
                }
            }
            */

            // Records give you all this functionality with just one line!
            Console.WriteLine("Records provide all the above functionality automatically!");
        }
    }

    /// <summary>
    /// Example of a more complex record with additional members
    /// </summary>
    public record ComplexProductEvent(
        Guid ProductId,
        string Name,
        decimal Price) : BaseDomainEvent
    {
        // You can still add additional properties and methods to records
        public bool IsExpensive => Price > 1000m;
        
        public string FormattedPrice => $"${Price:F2}";
        
        // Additional constructor
        public ComplexProductEvent(Guid productId, string name) 
            : this(productId, name, 0m) { }
        
        // Override ToString if you want custom formatting
        public override string ToString()
        {
            return $"Complex Product Event: {Name} (${Price:F2}) - {EventId}";
        }
    }
}
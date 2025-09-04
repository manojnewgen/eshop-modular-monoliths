using Microsoft.EntityFrameworkCore;
using Catalog.Data;
using Basket.Data;
using Catalog.Products;
using Basket.ShoppingCarts;

namespace Examples.SchemaIsolation
{
    /// <summary>
    /// Demonstration of Schema-Based Data Isolation for Modular Monolith
    /// Shows how CATALOG and BASKET modules maintain complete data separation
    /// </summary>
    public class SchemaIsolationDemo
    {
        private readonly CatalogDbContext _catalogContext;
        private readonly BasketDbContext _basketContext;

        public SchemaIsolationDemo(CatalogDbContext catalogContext, BasketDbContext basketContext)
        {
            _catalogContext = catalogContext;
            _basketContext = basketContext;
        }

        /// <summary>
        /// Demonstrates complete data isolation between modules
        /// </summary>
        public async Task DemonstrateDataIsolationAsync()
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("?? SCHEMA-BASED DATA ISOLATION DEMONSTRATION");
            Console.WriteLine("=================================================================");

            await ShowCatalogDataIsolation();
            await ShowBasketDataIsolation();
            await ShowCrossModuleInteraction();
            await ShowSecurityBenefits();
            await ShowPerformanceBenefits();
            await ShowEvolutionBenefits();

            Console.WriteLine("=================================================================");
            Console.WriteLine("? Schema isolation demonstration completed!");
            Console.WriteLine("=================================================================");
        }

        private async Task ShowCatalogDataIsolation()
        {
            Console.WriteLine("\n?? CATALOG MODULE - Schema: 'catalog'");
            Console.WriteLine("---------------------------------------------------");

            // Catalog module works ONLY with catalog schema
            var products = await _catalogContext.Products
                .Where(p => p.IsAvailable)
                .Take(3)
                .ToListAsync();

            Console.WriteLine($"? Catalog can access {products.Count} products from catalog.products");

            var categories = await _catalogContext.Categories
                .Take(3)
                .ToListAsync();

            Console.WriteLine($"? Catalog can access {categories.Count} categories from catalog.categories");

            // Show the actual SQL being generated with schema prefix
            var sqlQuery = _catalogContext.Products.ToQueryString();
            Console.WriteLine($"?? Generated SQL includes schema: {sqlQuery.Contains("catalog.products")}");

            // ? Catalog CANNOT access basket data (compile-time safety)
            // var carts = await _catalogContext.ShoppingCarts.ToListAsync(); // This won't compile!
            Console.WriteLine("?? Catalog module CANNOT access basket data (compile-time protection)");
        }

        private async Task ShowBasketDataIsolation()
        {
            Console.WriteLine("\n?? BASKET MODULE - Schema: 'basket'");
            Console.WriteLine("---------------------------------------------------");

            // Basket module works ONLY with basket schema
            var carts = await _basketContext.ShoppingCarts
                .Include(c => c.Items)
                .Take(3)
                .ToListAsync();

            Console.WriteLine($"? Basket can access {carts.Count} shopping carts from basket.shopping_carts");

            var totalItems = carts.SelectMany(c => c.Items).Count();
            Console.WriteLine($"? Basket can access {totalItems} cart items from basket.cart_items");

            // Show the actual SQL being generated with schema prefix
            var sqlQuery = _basketContext.ShoppingCarts.ToQueryString();
            Console.WriteLine($"?? Generated SQL includes schema: {sqlQuery.Contains("basket.shopping_carts")}");

            // ? Basket CANNOT access catalog data (compile-time safety)
            // var products = await _basketContext.Products.ToListAsync(); // This won't compile!
            Console.WriteLine("?? Basket module CANNOT access catalog data (compile-time protection)");
        }

        private async Task ShowCrossModuleInteraction()
        {
            Console.WriteLine("\n?? CROSS-MODULE INTERACTION - Loose Coupling");
            Console.WriteLine("---------------------------------------------------");

            // Example: Adding a product to cart (cross-module operation)
            var product = await _catalogContext.Products.FirstAsync();
            Console.WriteLine($"?? Found product in catalog: {product.Name} (ID: {product.Id})");

            // Create a shopping cart in basket module
            var userId = Guid.NewGuid();
            var cart = ShoppingCart.Create(userId);

            // Add product to cart using ProductId as reference (loose coupling)
            cart.AddItem(
                productId: product.Id,          // Reference to catalog data
                productName: product.Name,      // Denormalized for performance
                productPrice: product.Price,    // Denormalized for performance
                productImageUrl: product.ImageFile,
                quantity: 2,
                unitPrice: product.Price
            );

            _basketContext.ShoppingCarts.Add(cart);
            await _basketContext.SaveChangesAsync();

            Console.WriteLine($"? Product added to cart with loose coupling");
            Console.WriteLine($"   • Cart references product by ID: {product.Id}");
            Console.WriteLine($"   • Product data denormalized in basket for performance");
            Console.WriteLine($"   • No foreign key constraint between schemas");

            // Show that cart item contains denormalized product data
            var cartItem = cart.Items.First();
            Console.WriteLine($"   • Cart item name: {cartItem.ProductName}");
            Console.WriteLine($"   • Cart item price: {cartItem.ProductPrice:C}");
        }

        private async Task ShowSecurityBenefits()
        {
            Console.WriteLine("\n?? SECURITY BENEFITS");
            Console.WriteLine("---------------------------------------------------");

            Console.WriteLine("? Schema-level permissions:");
            Console.WriteLine("   • catalog_user can only access catalog schema");
            Console.WriteLine("   • basket_user can only access basket schema");
            Console.WriteLine("   • No accidental cross-module data access");
            Console.WriteLine("   • Database-level security enforcement");

            Console.WriteLine("\n? Code-level safety:");
            Console.WriteLine("   • DbContext prevents cross-module access at compile time");
            Console.WriteLine("   • Each module has its own connection string");
            Console.WriteLine("   • Clear boundaries between modules");

            // Demonstrate that each module has its own transaction scope
            using var catalogTransaction = await _catalogContext.Database.BeginTransactionAsync();
            using var basketTransaction = await _basketContext.Database.BeginTransactionAsync();

            Console.WriteLine("   • Independent transaction scopes per module");
            Console.WriteLine("   • Module failures don't affect other modules");

            await catalogTransaction.RollbackAsync();
            await basketTransaction.RollbackAsync();
        }

        private async Task ShowPerformanceBenefits()
        {
            Console.WriteLine("\n? PERFORMANCE BENEFITS");
            Console.WriteLine("---------------------------------------------------");

            // Show how schemas enable targeted indexing
            Console.WriteLine("? Schema-specific optimizations:");
            Console.WriteLine("   • catalog.products has indexes for product search");
            Console.WriteLine("   • basket.cart_items has indexes for cart operations");
            Console.WriteLine("   • No index interference between modules");

            // Demonstrate query performance
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            var productQuery = await _catalogContext.Products
                .Where(p => p.Price > 100)
                .CountAsync();
            
            var catalogTime = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();

            var cartQuery = await _basketContext.ShoppingCarts
                .Where(c => c.Status == "Active")
                .CountAsync();
            
            var basketTime = stopwatch.ElapsedMilliseconds;

            Console.WriteLine($"   • Catalog query: {catalogTime}ms (schema-optimized indexes)");
            Console.WriteLine($"   • Basket query: {basketTime}ms (schema-optimized indexes)");
            Console.WriteLine("   • No cross-schema query overhead");
        }

        private async Task ShowEvolutionBenefits()
        {
            Console.WriteLine("\n?? EVOLUTION & MIGRATION BENEFITS");
            Console.WriteLine("---------------------------------------------------");

            Console.WriteLine("? Independent schema evolution:");
            Console.WriteLine("   • Catalog schema can evolve independently");
            Console.WriteLine("   • Basket schema changes don't affect catalog");
            Console.WriteLine("   • Migration scripts are module-specific");

            Console.WriteLine("\n? Microservices-ready architecture:");
            Console.WriteLine("   • Each schema maps to a future microservice");
            Console.WriteLine("   • Data is already isolated and bounded");
            Console.WriteLine("   • Easy to extract to separate databases");

            Console.WriteLine("\n? Deployment flexibility:");
            Console.WriteLine("   • Module-specific database maintenance");
            Console.WriteLine("   • Independent backup strategies");
            Console.WriteLine("   • Selective data archiving per module");

            // Show connection string isolation
            var catalogConnection = _catalogContext.Database.GetConnectionString();
            var basketConnection = _basketContext.Database.GetConnectionString();

            Console.WriteLine($"\n?? Connection strings can be different:");
            Console.WriteLine($"   • Catalog: {catalogConnection?.Substring(0, 50)}...");
            Console.WriteLine($"   • Basket: {basketConnection?.Substring(0, 50)}...");
            Console.WriteLine("   • Ready for separate database instances");
        }

        /// <summary>
        /// Shows practical queries that demonstrate schema isolation
        /// </summary>
        public async Task ShowPracticalSchemaQueries()
        {
            Console.WriteLine("\n?? PRACTICAL SCHEMA QUERIES");
            Console.WriteLine("---------------------------------------------------");

            // Raw SQL queries showing schema prefixes
            var catalogProductCount = await _catalogContext.Database
                .SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM catalog.products WHERE is_available = true")
                .FirstAsync();

            var basketActiveCartsCount = await _basketContext.Database
                .SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM basket.shopping_carts WHERE status = 'Active'")
                .FirstAsync();

            Console.WriteLine($"? Catalog products: {catalogProductCount} (from catalog.products)");
            Console.WriteLine($"? Active carts: {basketActiveCartsCount} (from basket.shopping_carts)");

            // Show schema information queries
            var catalogTables = await _catalogContext.Database
                .SqlQueryRaw<string>("SELECT table_name FROM information_schema.tables WHERE table_schema = 'catalog'")
                .ToListAsync();

            var basketTables = await _basketContext.Database
                .SqlQueryRaw<string>("SELECT table_name FROM information_schema.tables WHERE table_schema = 'basket'")
                .ToListAsync();

            Console.WriteLine($"\n?? Catalog schema tables: [{string.Join(", ", catalogTables)}]");
            Console.WriteLine($"?? Basket schema tables: [{string.Join(", ", basketTables)}]");
        }
    }

    /// <summary>
    /// Extension class to show schema isolation configuration
    /// </summary>
    public static class SchemaIsolationExtensions
    {
        public static IServiceCollection AddSchemaIsolatedModules(this IServiceCollection services, IConfiguration configuration)
        {
            // Catalog module with its own DbContext and connection
            services.AddDbContext<CatalogDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("CatalogConnection"),
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "catalog")));

            // Basket module with its own DbContext and connection
            services.AddDbContext<BasketDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("BasketConnection"),
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "basket")));

            Console.WriteLine("? Schema isolation configured:");
            Console.WriteLine("   • Catalog: Uses 'catalog' schema with dedicated connection");
            Console.WriteLine("   • Basket: Uses 'basket' schema with dedicated connection");
            Console.WriteLine("   • Migration histories are schema-specific");

            return services;
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Basket.Basket.Modules;
using System.Reflection;

namespace Basket.Data
{
    /// <summary>
    /// Basket Module Database Context - Isolated to 'basket' schema
    /// </summary>
    public class BasketDbContext : DbContext
    {
        public BasketDbContext(DbContextOptions<BasketDbContext> options) : base(options) { }

        // Basket entities
        public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
        public DbSet<ShoppingCartItem> CartItems => Set<ShoppingCartItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ShoppingCart>().Property(c => c.UserName).IsRequired().HasMaxLength(100);

            builder.HasDefaultSchema("basket");
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(builder);
        }

      
    }
}
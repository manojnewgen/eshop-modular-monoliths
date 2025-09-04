using Microsoft.EntityFrameworkCore;
using Basket.ShoppingCarts;

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
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<CartDiscount> CartDiscounts => Set<CartDiscount>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ?? KEY ISOLATION STRATEGY: All tables in 'basket' schema
            modelBuilder.HasDefaultSchema("basket");

            ConfigureShoppingCartEntity(modelBuilder);
            ConfigureCartItemEntity(modelBuilder);
            ConfigureCartDiscountEntity(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private static void ConfigureShoppingCartEntity(ModelBuilder modelBuilder)
        {
            var cartBuilder = modelBuilder.Entity<ShoppingCart>();

            // Table configuration - explicitly in basket schema
            cartBuilder.ToTable("shopping_carts", "basket");

            cartBuilder.HasKey(c => c.Id);

            cartBuilder.Property(c => c.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            cartBuilder.Property(c => c.SessionId)
                .HasMaxLength(500)
                .HasColumnName("session_id");

            cartBuilder.Property(c => c.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Active")
                .IsRequired();

            cartBuilder.Property(c => c.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            cartBuilder.Property(c => c.LastModifiedAt)
                .HasColumnName("last_modified_at")
                .HasDefaultValueSql("NOW()");

            // Relationships
            cartBuilder.HasMany(c => c.Items)
                .WithOne(i => i.ShoppingCart)
                .HasForeignKey(i => i.CartId)
                .HasConstraintName("fk_cart_items_cart_id");

            cartBuilder.HasMany(c => c.Discounts)
                .WithOne(d => d.ShoppingCart)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("fk_cart_discounts_cart_id");

            // Indexes
            cartBuilder.HasIndex(c => c.UserId)
                .HasDatabaseName("idx_shopping_carts_user_id");

            cartBuilder.HasIndex(c => c.SessionId)
                .HasDatabaseName("idx_shopping_carts_session_id");
        }

        private static void ConfigureCartItemEntity(ModelBuilder modelBuilder)
        {
            var itemBuilder = modelBuilder.Entity<CartItem>();

            itemBuilder.ToTable("cart_items", "basket");
            itemBuilder.HasKey(i => i.Id);

            itemBuilder.Property(i => i.CartId)
                .HasColumnName("cart_id")
                .IsRequired();

            // ?? IMPORTANT: ProductId references catalog.products but NO foreign key
            // This maintains loose coupling between modules
            itemBuilder.Property(i => i.ProductId)
                .HasColumnName("product_id")
                .IsRequired();

            // Denormalized product data for performance and decoupling
            itemBuilder.Property(i => i.ProductName)
                .HasMaxLength(200)
                .HasColumnName("product_name")
                .IsRequired();

            itemBuilder.Property(i => i.ProductPrice)
                .HasColumnType("DECIMAL(18,2)")
                .HasColumnName("product_price")
                .IsRequired();

            itemBuilder.Property(i => i.ProductImageUrl)
                .HasMaxLength(500)
                .HasColumnName("product_image_url");

            itemBuilder.Property(i => i.Quantity)
                .IsRequired();

            itemBuilder.Property(i => i.UnitPrice)
                .HasColumnType("DECIMAL(18,2)")
                .HasColumnName("unit_price")
                .IsRequired();

            // Computed column for total price
            itemBuilder.Property(i => i.TotalPrice)
                .HasColumnType("DECIMAL(18,2)")
                .HasColumnName("total_price")
                .HasComputedColumnSql("quantity * unit_price", stored: true);

            itemBuilder.Property(i => i.AddedAt)
                .HasColumnName("added_at")
                .HasDefaultValueSql("NOW()");

            itemBuilder.Property(i => i.LastModifiedAt)
                .HasColumnName("last_modified_at")
                .HasDefaultValueSql("NOW()");

            // Ensure unique product per cart
            itemBuilder.HasIndex(i => new { i.CartId, i.ProductId })
                .IsUnique()
                .HasDatabaseName("idx_cart_items_unique");

            itemBuilder.HasIndex(i => i.ProductId)
                .HasDatabaseName("idx_cart_items_product_id");
        }

        private static void ConfigureCartDiscountEntity(ModelBuilder modelBuilder)
        {
            var discountBuilder = modelBuilder.Entity<CartDiscount>();

            discountBuilder.ToTable("cart_discounts", "basket");
            discountBuilder.HasKey(d => d.Id);

            discountBuilder.Property(d => d.CartId)
                .HasColumnName("cart_id")
                .IsRequired();

            discountBuilder.Property(d => d.DiscountCode)
                .HasMaxLength(50)
                .HasColumnName("discount_code")
                .IsRequired();

            discountBuilder.Property(d => d.DiscountType)
                .HasMaxLength(20)
                .HasColumnName("discount_type")
                .IsRequired();

            discountBuilder.Property(d => d.DiscountValue)
                .HasColumnType("DECIMAL(18,2)")
                .HasColumnName("discount_value")
                .IsRequired();

            discountBuilder.Property(d => d.AppliedAt)
                .HasColumnName("applied_at")
                .HasDefaultValueSql("NOW()");
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Catalog.Products;
using System.Reflection;

namespace Catalog.Data
{
    /// <summary>
    /// Catalog Module Database Context - Isolated to 'catalog' schema
    /// </summary>
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

        // Catalog entities
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ?? KEY ISOLATION STRATEGY: All tables in 'catalog' schema
            modelBuilder.HasDefaultSchema("catalog");

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }

        private static void ConfigureProductEntity(ModelBuilder modelBuilder)
        {
            var productBuilder = modelBuilder.Entity<Product>();

            // Table configuration - explicitly in catalog schema
            productBuilder.ToTable("products", "catalog");

            // Primary key
            productBuilder.HasKey(p => p.Id);

            // Properties
            productBuilder.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();

            productBuilder.Property(p => p.Description)
                .HasColumnType("TEXT");

            productBuilder.Property(p => p.Price)
                .HasColumnType("DECIMAL(18,2)")
                .IsRequired();

            productBuilder.Property(p => p.ImageFile)
                .HasMaxLength(500)
                .HasColumnName("image_file");

            // Categories as array (PostgreSQL specific)
            productBuilder.Property(p => p.Categories)
                .HasConversion(
                    v => v.ToArray(),
                    v => v.ToList())
                .HasColumnName("categories");

            // Audit fields
            productBuilder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            productBuilder.Property(p => p.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");

            productBuilder.Property(p => p.LastModifiedAt)
                .HasColumnName("last_modified_at");

            productBuilder.Property(p => p.LastModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("last_modified_by");

            // Indexes
            productBuilder.HasIndex(p => p.Name)
                .HasDatabaseName("idx_products_name");

            productBuilder.HasIndex(p => p.Price)
                .HasDatabaseName("idx_products_price");
        }

        private static void ConfigureCategoryEntity(ModelBuilder modelBuilder)
        {
            var categoryBuilder = modelBuilder.Entity<Category>();

            categoryBuilder.ToTable("categories", "catalog");
            categoryBuilder.HasKey(c => c.Id);

            categoryBuilder.Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();

            categoryBuilder.Property(c => c.Description)
                .HasColumnType("TEXT");

            categoryBuilder.Property(c => c.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            // Self-referencing relationship for parent category
            categoryBuilder.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .HasConstraintName("fk_categories_parent");

            categoryBuilder.HasIndex(c => c.Name)
                .IsUnique()
                .HasDatabaseName("idx_categories_name_unique");
        }
    }

    // Category entity for the example
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Category? ParentCategory { get; set; }
        public List<Category> SubCategories { get; set; } = new();
    }
}
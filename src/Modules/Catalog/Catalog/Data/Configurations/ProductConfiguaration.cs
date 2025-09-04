using Catalog.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.DDD;

namespace Catalog.Data.Configurations
{
    public class ProductConfiguaration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // Table configuration - explicitly in catalog schema
            builder.ToTable("products", "catalog");
            
            // Primary key
            builder.HasKey(p => p.Id);
            
            // Basic properties
            builder.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();
                
            builder.Property(p => p.Description)
                .HasColumnType("TEXT");
                
            builder.Property(p => p.Price)
                .HasColumnType("DECIMAL(18,2)")
                .IsRequired();
                
            builder.Property(p => p.ImageFile)
                .HasMaxLength(500)
                .HasColumnName("image_file");
                
            // Categories as array (PostgreSQL specific)
            builder.Property(p => p.Categories)
                .HasConversion(
                    v => v.ToArray(),
                    v => v.ToList())
                .HasColumnName("categories");
                
            // Additional properties for database compatibility
            builder.Property(p => p.StockQuantity)
                .IsRequired()
                .HasDefaultValue(0);
                
            builder.Property(p => p.IsAvailable)
                .IsRequired()
                .HasDefaultValue(true);

            // 🆕 Audit fields configuration (inherited from Entity<T>)
            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at");
                
            builder.Property(p => p.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
                
            builder.Property(p => p.LastModifiedAt)
                .HasColumnName("last_modified_at");
                
            builder.Property(p => p.LastModifiedBy)
                .HasMaxLength(100)
                .HasColumnName("last_modified_by");

            // 🆕 Soft delete fields configuration
            builder.Property(p => p.IsDeleted)
                .HasColumnName("is_deleted")
                .IsRequired()
                .HasDefaultValue(false);
                
            builder.Property(p => p.DeletedAt)
                .HasColumnName("deleted_at");
                
            builder.Property(p => p.DeletedBy)
                .HasMaxLength(100)
                .HasColumnName("deleted_by");

            // 🆕 Query filter to exclude soft-deleted items by default
            builder.HasQueryFilter(p => !p.IsDeleted);

            // Indexes
            builder.HasIndex(p => p.Name)
                .HasDatabaseName("idx_products_name");
                
            builder.HasIndex(p => p.Price)
                .HasDatabaseName("idx_products_price");
                
            builder.HasIndex(p => p.IsDeleted)
                .HasDatabaseName("idx_products_is_deleted");
                
            builder.HasIndex(p => p.CreatedAt)
                .HasDatabaseName("idx_products_created_at");

            // 🆕 Ignore domain events for EF Core (they're handled by the aggregate base class)
            builder.Ignore(p => p.DomainEvents);
            builder.Ignore(p => p.Events);
        }
    }
}

using Basket.Basket.Modules;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Security.Cryptography;

namespace Basket.Data.Configurations
{
    public class ShoppingCartItemConfiguration : IEntityTypeConfiguration<ShoppingCartItem>
    {
        public void Configure(EntityTypeBuilder<ShoppingCartItem> builder)
        {
            builder.HasIndex(e => e.Id);
            builder.Property(i => i.ProductId).IsRequired();
            builder.Property(i => i.ProductName).IsRequired();
            builder.Property(i => i.Color);
            builder.Property(i => i.Price).IsRequired();
            builder.Property(i => i.Quantity).IsRequired();
        }
    }
}

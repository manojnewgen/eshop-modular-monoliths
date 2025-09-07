using Basket.Basket.Modules;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Basket.Data.Configurations
{
    public class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCart>
    {
        public void Configure(EntityTypeBuilder<ShoppingCart> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(e => e.UserName).IsUnique();

            builder.Property(e=>e.UserName).IsRequired().HasMaxLength(128);




        }
    }
}

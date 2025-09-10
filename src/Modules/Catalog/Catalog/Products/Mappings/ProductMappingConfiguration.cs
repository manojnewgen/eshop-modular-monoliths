using Mapster;
using Catalog.Products;
using Catalog.Products.Dtos;
using Shared.Mapping;

namespace Catalog.Products.Mappings
{
    /// <summary>
    /// Mapster configuration for Product entity to ProductDto mapping
    /// </summary>
    public class ProductMappingConfiguration : IMappingConfiguration
    {
        public void Configure(TypeAdapterConfig config)
        {
            // Product to ProductDto mapping
            config.NewConfig<Product, ProductDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.ImageFile, src => src.ImageFile)
                .Map(dest => dest.Categories, src => src.Categories.ToList())
                .Map(dest => dest.StockQuantity, src => src.StockQuantity);

            // ProductDto to Product mapping (for creation scenarios only)
            config.NewConfig<ProductDto, Product>()
                .ConstructUsing(src => Product.Create(
                    src.Id == Guid.Empty ? Guid.NewGuid() : src.Id,
                    src.Name ?? string.Empty,
                    src.Description ?? string.Empty,
                    src.Price,
                    src.ImageFile ?? string.Empty,
                    src.Categories ?? new List<string>(),
                    src.StockQuantity
                ))
                .PreserveReference(true);

            // IQueryable<Product> to IQueryable<ProductDto> for efficient database projections
            config.NewConfig<Product, ProductDto>()
                .Map(dest => dest.Categories, src => src.Categories.ToList())
                .PreserveReference(true);
        }
    }
}
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

            // ProductDto to Product mapping (for creation scenarios)
            config.NewConfig<ProductDto, Product>()
                .ConstructUsing(src => Product.Create(
                    src.Id == Guid.Empty ? Guid.NewGuid() : src.Id,
                    src.Name,
                    src.Description,
                    src.Price,
                    src.ImageFile,
                    src.Categories,
                    src.StockQuantity
                ))
                .PreserveReference(true);

            // IQueryable<Product> to IQueryable<ProductDto> for efficient database projections
            config.NewConfig<Product, ProductDto>()
                .Map(dest => dest.Categories, src => src.Categories.ToList())
                .PreserveReference(true);

            // Additional mapping for Product updates - maps only changed fields
            config.NewConfig<ProductDto, Product>()
                .Ignore(dest => dest.Id) // Never map ID for updates
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.CreatedBy)
                .Ignore(dest => dest.LastModifiedAt)
                .Ignore(dest => dest.LastModifiedBy)
                .Ignore(dest => dest.IsDeleted)
                .Ignore(dest => dest.DeletedAt)
                .Ignore(dest => dest.DeletedBy)
                .Ignore(dest => dest.Events) // Domain events
                .AfterMapping((src, dest) =>
                {
                    // Use domain methods for updates to ensure business logic and events
                    if (dest.Name != src.Name)
                        dest.UpdateName(src.Name);
                    
                    if (dest.Description != src.Description)
                        dest.UpdateDescription(src.Description);
                    
                    if (dest.Price != src.Price)
                        dest.UpdatePrice(src.Price, "Updated via API");
                    
                    if (dest.ImageFile != src.ImageFile)
                        dest.UpdateImageFile(src.ImageFile);
                    
                    if (dest.StockQuantity != src.StockQuantity)
                        dest.UpdateStock(src.StockQuantity);

                    // Handle categories
                    var currentCategories = dest.Categories.ToList();
                    if (!currentCategories.SequenceEqual(src.Categories))
                    {
                        dest.ClearCategories();
                        if (src.Categories.Any())
                        {
                            dest.AddCategories(src.Categories);
                        }
                    }
                });
        }
    }
}
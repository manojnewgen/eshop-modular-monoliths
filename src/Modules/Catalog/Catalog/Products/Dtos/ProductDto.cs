namespace Catalog.Products.Dtos
{
    /// <summary>
    /// Product Data Transfer Object for API requests and responses
    /// </summary>
    public record ProductDto(
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        string ImageFile,
        List<string> Categories,
        int StockQuantity
    );
}

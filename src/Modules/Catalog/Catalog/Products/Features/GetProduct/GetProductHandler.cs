

namespace Catalog.Products.Features.GetProduct
{
    /// <summary>
    /// Query to retrieve a single product by ID
    /// </summary>
    public record GetProductQuery(Guid ProductId) : IQuery<ProductDto?>;

    /// <summary>
    /// Handler for GetProductQuery - retrieves a single product by ID using Mapster
    /// </summary>
    public class GetProductHandler : IQueryHandler<GetProductQuery, ProductDto?>
    {
        private readonly CatalogDbContext _dbContext;
        private readonly ILogger<GetProductHandler> _logger;
        private readonly IMappingService _mappingService;

        public GetProductHandler(
            CatalogDbContext dbContext, 
            ILogger<GetProductHandler> logger,
            IMappingService mappingService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _mappingService = mappingService;
        }

        public async Task<ProductDto?> Handle(GetProductQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting product with ID: {ProductId}", query.ProductId);

            try
            {
                // Use Mapster projection for efficient database query
                var productDto = await _mappingService
                    .ProjectToType<ProductDto>(_dbContext.Products.Where(p => p.Id == query.ProductId))
                    .FirstOrDefaultAsync(cancellationToken);

                if (productDto == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", query.ProductId);
                }
                else
                {
                    _logger.LogInformation("Retrieved product: {ProductName}", productDto.Name);
                }

                return productDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product: {ProductId}", query.ProductId);
                throw;
            }
        }
    }
}
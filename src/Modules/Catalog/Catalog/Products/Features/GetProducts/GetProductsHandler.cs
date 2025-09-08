

namespace Catalog.Products.Features.GetProducts
{
    /// <summary>
    /// Query to retrieve a list of products with optional filtering and pagination
    /// </summary>
    public record GetProductsQuery(
        string? SearchTerm = null,
        string? Category = null,
        decimal? MinPrice = null,
        decimal? MaxPrice = null,
        bool IncludeDeleted = false,
        int PageNumber = 1,
        int PageSize = 20
    ) : IQuery<GetProductsResult>;

    /// <summary>
    /// Result containing the list of products and pagination info
    /// </summary>
    public record GetProductsResult(
        List<ProductDto> Products,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages
    );

    /// <summary>
    /// Handler for GetProductsQuery - retrieves products with filtering and pagination using Mapster
    /// </summary>
    public class GetProductsHandler : IQueryHandler<GetProductsQuery, GetProductsResult>
    {
        private readonly CatalogDbContext _dbContext;
        private readonly ILogger<GetProductsHandler> _logger;
        private readonly IMappingService _mappingService;

        public GetProductsHandler(
            CatalogDbContext dbContext, 
            ILogger<GetProductsHandler> logger,
            IMappingService mappingService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _mappingService = mappingService;
        }

        public async Task<GetProductsResult> Handle(GetProductsQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting products with filters - SearchTerm: {SearchTerm}, Category: {Category}, Page: {PageNumber}", 
                query.SearchTerm, query.Category, query.PageNumber);

            try
            {
                // Start with the base query
                var baseQuery = _dbContext.Products.AsQueryable();

                // Include soft-deleted items if requested
                if (query.IncludeDeleted)
                {
                    baseQuery = baseQuery.IgnoreQueryFilters();
                }

                // Apply filters
                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    baseQuery = baseQuery.Where(p => 
                        p.Name.Contains(query.SearchTerm) || 
                        p.Description.Contains(query.SearchTerm));
                }

                if (!string.IsNullOrWhiteSpace(query.Category))
                {
                    baseQuery = baseQuery.Where(p => p.Categories.Contains(query.Category));
                }

                if (query.MinPrice.HasValue)
                {
                    baseQuery = baseQuery.Where(p => p.Price >= query.MinPrice.Value);
                }

                if (query.MaxPrice.HasValue)
                {
                    baseQuery = baseQuery.Where(p => p.Price <= query.MaxPrice.Value);
                }

                // Get total count for pagination
                var totalCount = await baseQuery.CountAsync(cancellationToken);

                // Calculate pagination
                var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);
                var skip = (query.PageNumber - 1) * query.PageSize;

                // Use Mapster projection for efficient database query
                var products = await _mappingService
                    .ProjectToType<ProductDto>(baseQuery.OrderBy(p => p.Name).Skip(skip).Take(query.PageSize))
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {ProductCount} products out of {TotalCount} total using Mapster projection", 
                    products.Count, totalCount);

                return new GetProductsResult(
                    Products: products,
                    TotalCount: totalCount,
                    PageNumber: query.PageNumber,
                    PageSize: query.PageSize,
                    TotalPages: totalPages
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                throw;
            }
        }
    }
}
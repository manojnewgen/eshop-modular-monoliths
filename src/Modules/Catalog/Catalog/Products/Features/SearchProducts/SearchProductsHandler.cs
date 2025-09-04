using Catalog.Products.Dtos;
using Catalog.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Shared.CQRS;
using Shared.Mapping;

namespace Catalog.Products.Features.SearchProducts
{
    /// <summary>
    /// Query to search products with advanced filtering
    /// </summary>
    public record SearchProductsQuery(
        string SearchTerm,
        List<string>? Categories = null,
        decimal? MinPrice = null,
        decimal? MaxPrice = null,
        bool? InStock = null,
        int PageNumber = 1,
        int PageSize = 20
    ) : IQuery<SearchProductsResult>;

    /// <summary>
    /// Result containing search results
    /// </summary>
    public record SearchProductsResult(
        string SearchTerm,
        List<ProductDto> Products,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages
    );

    /// <summary>
    /// Handler for SearchProductsQuery - performs advanced product search using Mapster
    /// </summary>
    public class SearchProductsHandler : IQueryHandler<SearchProductsQuery, SearchProductsResult>
    {
        private readonly CatalogDbContext _dbContext;
        private readonly ILogger<SearchProductsHandler> _logger;
        private readonly IMappingService _mappingService;

        public SearchProductsHandler(
            CatalogDbContext dbContext, 
            ILogger<SearchProductsHandler> logger,
            IMappingService mappingService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _mappingService = mappingService;
        }

        public async Task<SearchProductsResult> Handle(SearchProductsQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Searching products with term: '{SearchTerm}', Categories: [{Categories}] using Mapster", 
                query.SearchTerm, query.Categories != null ? string.Join(", ", query.Categories) : "All");

            try
            {
                // Start with base query
                var baseQuery = _dbContext.Products.AsQueryable();

                // Apply search term filter
                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    baseQuery = baseQuery.Where(p => 
                        p.Name.Contains(query.SearchTerm) || 
                        p.Description.Contains(query.SearchTerm) ||
                        p.Categories.Any(c => c.Contains(query.SearchTerm)));
                }

                // Apply category filters
                if (query.Categories != null && query.Categories.Any())
                {
                    baseQuery = baseQuery.Where(p => 
                        query.Categories.Any(category => p.Categories.Contains(category)));
                }

                // Apply price range filters
                if (query.MinPrice.HasValue)
                {
                    baseQuery = baseQuery.Where(p => p.Price >= query.MinPrice.Value);
                }

                if (query.MaxPrice.HasValue)
                {
                    baseQuery = baseQuery.Where(p => p.Price <= query.MaxPrice.Value);
                }

                // Apply stock filter
                if (query.InStock.HasValue)
                {
                    if (query.InStock.Value)
                    {
                        baseQuery = baseQuery.Where(p => p.StockQuantity > 0 && p.IsAvailable);
                    }
                    else
                    {
                        baseQuery = baseQuery.Where(p => p.StockQuantity == 0 || !p.IsAvailable);
                    }
                }

                // Get total count for pagination
                var totalCount = await baseQuery.CountAsync(cancellationToken);

                // Calculate pagination
                var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);
                var skip = (query.PageNumber - 1) * query.PageSize;

                // Use Mapster projection for efficient database query with relevance ordering
                var products = await _mappingService
                    .ProjectToType<ProductDto>(
                        baseQuery
                            .OrderByDescending(p => p.Name.Contains(query.SearchTerm ?? "")) // Relevance
                            .ThenBy(p => p.Price) // Secondary sort
                            .Skip(skip)
                            .Take(query.PageSize)
                    )
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Search completed using Mapster: Found {ProductCount} products out of {TotalCount} total for term '{SearchTerm}'", 
                    products.Count, totalCount, query.SearchTerm);

                return new SearchProductsResult(
                    SearchTerm: query.SearchTerm ?? "",
                    Products: products,
                    TotalCount: totalCount,
                    PageNumber: query.PageNumber,
                    PageSize: query.PageSize,
                    TotalPages: totalPages
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products with term: {SearchTerm}", query.SearchTerm);
                throw;
            }
        }
    }
}
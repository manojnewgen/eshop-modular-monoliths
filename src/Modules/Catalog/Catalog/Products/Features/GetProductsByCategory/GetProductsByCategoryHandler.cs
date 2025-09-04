using Catalog.Products.Dtos;
using Catalog.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Shared.CQRS;

namespace Catalog.Products.Features.GetProductsByCategory
{
    /// <summary>
    /// Query to retrieve products by category
    /// </summary>
    public record GetProductsByCategoryQuery(
        string Category,
        int PageNumber = 1,
        int PageSize = 20
    ) : IQuery<GetProductsByCategoryResult>;

    /// <summary>
    /// Result containing products in a specific category
    /// </summary>
    public record GetProductsByCategoryResult(
        string Category,
        List<ProductDto> Products,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages
    );

    /// <summary>
    /// Handler for GetProductsByCategoryQuery - retrieves products by category
    /// </summary>
    public class GetProductsByCategoryHandler : IQueryHandler<GetProductsByCategoryQuery, GetProductsByCategoryResult>
    {
        private readonly CatalogDbContext _dbContext;
        private readonly ILogger<GetProductsByCategoryHandler> _logger;

        public GetProductsByCategoryHandler(CatalogDbContext dbContext, ILogger<GetProductsByCategoryHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<GetProductsByCategoryResult> Handle(GetProductsByCategoryQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting products by category: {Category}, Page: {PageNumber}", query.Category, query.PageNumber);

            try
            {
                // Filter products by category
                var baseQuery = _dbContext.Products
                    .Where(p => p.Categories.Contains(query.Category));

                // Get total count for pagination
                var totalCount = await baseQuery.CountAsync(cancellationToken);

                // Calculate pagination
                var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);
                var skip = (query.PageNumber - 1) * query.PageSize;

                // Get the products with pagination
                var products = await baseQuery
                    .OrderBy(p => p.Name)
                    .Skip(skip)
                    .Take(query.PageSize)
                    .Select(p => new ProductDto(
                        p.Id,
                        p.Name,
                        p.Description,
                        p.Price,
                        p.ImageFile,
                        p.Categories.ToList(),
                        p.StockQuantity
                    ))
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {ProductCount} products in category '{Category}' out of {TotalCount} total", 
                    products.Count, query.Category, totalCount);

                return new GetProductsByCategoryResult(
                    Category: query.Category,
                    Products: products,
                    TotalCount: totalCount,
                    PageNumber: query.PageNumber,
                    PageSize: query.PageSize,
                    TotalPages: totalPages
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products by category: {Category}", query.Category);
                throw;
            }
        }
    }
}
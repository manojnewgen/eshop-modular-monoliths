using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MediatR;
using Catalog.Products.Features.CreateProduct;
using Catalog.Products.Features.UpdateProduct;
using Catalog.Products.Features.GetProduct;
using Catalog.Products.Features.GetProducts;
using Catalog.Products.Features.DeleteProduct;
using Catalog.Products.Features.RestoreProduct;
using Catalog.Products.Features.UpdateProductPrice;
using Catalog.Products.Features.UpdateProductStock;
using Catalog.Products.Features.GetProductsByCategory;
using Catalog.Products.Features.SearchProducts;
using Catalog.Products.Dtos;

namespace Catalog.Products.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all products with optional filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<GetProductsResult>> GetProducts(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? category = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] bool includeDeleted = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = new GetProductsQuery(searchTerm, category, minPrice, maxPrice, includeDeleted, pageNumber, pageSize);
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return BadRequest($"Error getting products: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a single product by ID
        /// </summary>
        [HttpGet("{id:guid}", Name = "GetProduct")]
        public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
        {
            try
            {
                var query = new GetProductQuery(id);
                var result = await _mediator.Send(query);
                
                if (result == null)
                {
                    return NotFound($"Product with ID {id} not found");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product: {ProductId}", id);
                return BadRequest($"Error getting product: {ex.Message}");
            }
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        [HttpGet("category/{category}")]
        public async Task<ActionResult<GetProductsByCategoryResult>> GetProductsByCategory(
            string category,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = new GetProductsByCategoryQuery(category, pageNumber, pageSize);
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category: {Category}", category);
                return BadRequest($"Error getting products by category: {ex.Message}");
            }
        }

        /// <summary>
        /// Search products with advanced filtering
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<SearchProductsResult>> SearchProducts(
            [FromQuery] string searchTerm,
            [FromQuery] List<string>? categories = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] bool? inStock = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = new SearchProductsQuery(searchTerm, categories, minPrice, maxPrice, inStock, pageNumber, pageSize);
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products with term: {SearchTerm}", searchTerm);
                return BadRequest($"Error searching products: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new product using ProductDto
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CreateProductResult>> CreateProduct([FromBody] ProductDto productDto)
        {
            try
            {
                var command = new CreateProductCommand(productDto);
                var result = await _mediator.Send(command);

                _logger.LogInformation("Product created via API with ID: {ProductId}", result.ProductId);
                return CreatedAtRoute("GetProduct", new { id = result.ProductId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product via API");
                return BadRequest($"Error creating product: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing product using ProductDto
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<UpdateProductResult>> UpdateProduct(Guid id, [FromBody] ProductDto productDto)
        {
            try
            {
                var command = new UpdateProductCommand(id, productDto);
                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    if (result.Message?.Contains("not found") == true)
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                _logger.LogInformation("Product updated via API: {ProductId}", result.ProductId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product via API: {ProductId}", id);
                return BadRequest($"Error updating product: {ex.Message}");
            }
        }

        /// <summary>
        /// Update product price specifically
        /// </summary>
        [HttpPatch("{id:guid}/price")]
        public async Task<ActionResult<UpdateProductPriceResult>> UpdateProductPrice(
            Guid id, 
            [FromBody] UpdatePriceRequest request)
        {
            try
            {
                var command = new UpdateProductPriceCommand(id, request.NewPrice, request.Reason);
                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    if (result.Message?.Contains("not found") == true)
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product price: {ProductId}", id);
                return BadRequest($"Error updating product price: {ex.Message}");
            }
        }

        /// <summary>
        /// Update product stock specifically
        /// </summary>
        [HttpPatch("{id:guid}/stock")]
        public async Task<ActionResult<UpdateProductStockResult>> UpdateProductStock(
            Guid id, 
            [FromBody] UpdateStockRequest request)
        {
            try
            {
                var command = new UpdateProductStockCommand(id, request.NewQuantity);
                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    if (result.Message?.Contains("not found") == true)
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product stock: {ProductId}", id);
                return BadRequest($"Error updating product stock: {ex.Message}");
            }
        }

        /// <summary>
        /// Soft delete a product
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<DeleteProductResult>> DeleteProduct(Guid id)
        {
            try
            {
                var command = new DeleteProductCommand(id);
                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    if (result.Message?.Contains("not found") == true)
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product: {ProductId}", id);
                return BadRequest($"Error deleting product: {ex.Message}");
            }
        }

        /// <summary>
        /// Restore a soft-deleted product
        /// </summary>
        [HttpPost("{id:guid}/restore")]
        public async Task<ActionResult<RestoreProductResult>> RestoreProduct(Guid id)
        {
            try
            {
                var command = new RestoreProductCommand(id);
                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    if (result.Message?.Contains("not found") == true)
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring product: {ProductId}", id);
                return BadRequest($"Error restoring product: {ex.Message}");
            }
        }
    }

    // Request DTOs
    public record UpdatePriceRequest(decimal NewPrice, string? Reason = null);
    public record UpdateStockRequest(int NewQuantity);
}
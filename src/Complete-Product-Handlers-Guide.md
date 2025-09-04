# ??? Complete Product Handlers - Catalog Features

## ?? **Overview**

All product handlers have been implemented in the `Modules/Catalog/Catalog/Products/Features/` directory following the CQRS pattern with MediatR integration and SaveChanges interceptor compatibility.

## ?? **Handlers Implemented**

### **?? Query Handlers (Read Operations)**

| Handler | Location | Purpose | HTTP Endpoint |
|---------|----------|---------|---------------|
| **GetProductHandler** | `GetProduct/` | Get single product by ID | `GET /api/products/{id}` |
| **GetProductsHandler** | `GetProducts/` | Get products with filtering & pagination | `GET /api/products` |
| **GetProductsByCategoryHandler** | `GetProductsByCategory/` | Get products by specific category | `GET /api/products/category/{category}` |
| **SearchProductsHandler** | `SearchProducts/` | Advanced product search | `GET /api/products/search` |

### **?? Command Handlers (Write Operations)**

| Handler | Location | Purpose | HTTP Endpoint |
|---------|----------|---------|---------------|
| **CreateProductHandler** | `CreateProduct/` | Create new product | `POST /api/products` |
| **UpdateProductHandler** | `UpdateProduct/` | Update entire product | `PUT /api/products/{id}` |
| **UpdateProductPriceHandler** | `UpdateProductPrice/` | Update product price only | `PATCH /api/products/{id}/price` |
| **UpdateProductStockHandler** | `UpdateProductStock/` | Update stock quantity only | `PATCH /api/products/{id}/stock` |
| **DeleteProductHandler** | `DeleteProduct/` | Soft delete product | `DELETE /api/products/{id}` |
| **RestoreProductHandler** | `RestoreProduct/` | Restore soft-deleted product | `POST /api/products/{id}/restore` |

## ?? **API Endpoints Summary**

### **Query Endpoints**

```http
# Get all products with optional filtering
GET /api/products?searchTerm=laptop&category=Gaming&minPrice=1000&maxPrice=2000&pageNumber=1&pageSize=10

# Get single product
GET /api/products/123e4567-e89b-12d3-a456-426614174000

# Get products by category
GET /api/products/category/Gaming?pageNumber=1&pageSize=10

# Advanced search
GET /api/products/search?searchTerm=laptop&categories=Gaming,Electronics&inStock=true&pageNumber=1&pageSize=10
```

### **Command Endpoints**

```http
# Create product
POST /api/products
Content-Type: application/json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "name": "Gaming Laptop",
  "description": "High-performance gaming laptop",
  "price": 1299.99,
  "imageFile": "laptop.jpg",
  "categories": ["Electronics", "Gaming"],
  "stockQuantity": 10
}

# Update entire product
PUT /api/products/123e4567-e89b-12d3-a456-426614174000
Content-Type: application/json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "Updated Gaming Laptop",
  "description": "Enhanced gaming laptop",
  "price": 1499.99,
  "imageFile": "updated-laptop.jpg",
  "categories": ["Electronics", "Gaming", "High-End"],
  "stockQuantity": 15
}

# Update price only
PATCH /api/products/123e4567-e89b-12d3-a456-426614174000/price
Content-Type: application/json
{
  "newPrice": 1199.99,
  "reason": "Black Friday discount"
}

# Update stock only
PATCH /api/products/123e4567-e89b-12d3-a456-426614174000/stock
Content-Type: application/json
{
  "newQuantity": 25
}

# Soft delete product
DELETE /api/products/123e4567-e89b-12d3-a456-426614174000

# Restore product
POST /api/products/123e4567-e89b-12d3-a456-426614174000/restore
```

## ?? **Key Features**

### **?? CQRS Pattern Implementation**
- ? **Separation of Concerns** - Queries and Commands in separate handlers
- ? **Single Responsibility** - Each handler does one thing well
- ? **Type Safety** - Strongly typed with generics
- ? **Testability** - Easy to unit test each handler independently

### **?? MediatR Integration**
- ? **Automatic Routing** - MediatR finds and calls correct handlers
- ? **Decoupling** - Controllers don't directly depend on handlers
- ? **Pipeline Behaviors** - Can add validation, caching, logging middleware
- ? **Event Dispatching** - Domain events automatically dispatched

### **? SaveChanges Interceptor Benefits**
All command handlers automatically get:
- ? **Audit Fields** - CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy
- ? **Soft Deletes** - Hard deletes converted to soft deletes
- ? **Domain Events** - Automatic collection and dispatching
- ? **Logging** - Structured logging for all operations
- ? **Error Handling** - Consistent error handling and rollback

### **?? Advanced Query Features**
- ? **Filtering** - By category, price range, search term
- ? **Pagination** - Page number and size with total counts
- ? **Sorting** - Intelligent sorting (relevance for search)
- ? **Soft Delete Handling** - Option to include/exclude deleted items
- ? **Performance** - Projection to DTOs, minimal data transfer

## ?? **Example Usage in Code**

```csharp
// Inject IMediator in your service/controller
public class ProductService
{
    private readonly IMediator _mediator;
    
    public ProductService(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    // Create product
    public async Task<Guid> CreateProductAsync(ProductDto product)
    {
        var command = new CreateProductCommand(product);
        var result = await _mediator.Send(command);
        return result.ProductId;
    }
    
    // Get product
    public async Task<ProductDto?> GetProductAsync(Guid id)
    {
        var query = new GetProductQuery(id);
        return await _mediator.Send(query);
    }
    
    // Search products
    public async Task<List<ProductDto>> SearchProductsAsync(string term)
    {
        var query = new SearchProductsQuery(term);
        var result = await _mediator.Send(query);
        return result.Products;
    }
    
    // Update price
    public async Task<bool> UpdatePriceAsync(Guid id, decimal newPrice)
    {
        var command = new UpdateProductPriceCommand(id, newPrice, "API update");
        var result = await _mediator.Send(command);
        return result.Success;
    }
}
```

## ?? **Domain Events Triggered**

Each handler automatically triggers relevant domain events:

| Handler | Domain Events |
|---------|---------------|
| **CreateProductHandler** | `ProductCreatedEvent` |
| **UpdateProductHandler** | `ProductPriceChangedEvent`, `ProductCategoriesUpdatedEvent` |
| **UpdateProductPriceHandler** | `ProductPriceChangedEvent` |
| **DeleteProductHandler** | `ProductDeletedEvent` |
| **RestoreProductHandler** | `ProductRestoredEvent` |

## ?? **File Structure**

```
Modules/Catalog/Catalog/Products/Features/
??? CreateProduct/
?   ??? CreateProductHandler.cs
??? UpdateProduct/
?   ??? UpdateProductHandler.cs
??? GetProduct/
?   ??? GetProductHandler.cs
??? GetProducts/
?   ??? GetProductsHandler.cs
??? DeleteProduct/
?   ??? DeleteProductHandler.cs
??? RestoreProduct/
?   ??? RestoreProductHandler.cs
??? UpdateProductPrice/
?   ??? UpdateProductPriceHandler.cs
??? UpdateProductStock/
?   ??? UpdateProductStockHandler.cs
??? GetProductsByCategory/
?   ??? GetProductsByCategoryHandler.cs
??? SearchProducts/
    ??? SearchProductsHandler.cs
```

## ?? **Benefits Summary**

1. **??? Complete CRUD** - All basic operations covered
2. **?? Advanced Search** - Powerful filtering and search capabilities  
3. **? High Performance** - Optimized queries with projections
4. **??? Type Safety** - Compile-time checking with generics
5. **?? Event-Driven** - Domain events for all state changes
6. **?? Audit Trail** - Automatic audit fields for compliance
7. **??? Soft Deletes** - Data preservation with restore capability
8. **?? Testable** - Easy unit testing for each operation
9. **?? Maintainable** - Clean, organized code structure
10. **?? Scalable** - Easy to add new features and handlers

Your Catalog module now has a **complete, production-ready set of handlers** that follow best practices and integrate seamlessly with your CQRS + MediatR + SaveChanges interceptor architecture! ??
# ?? Custom Exception Handler Implementation Guide

## ?? **Overview**

The `CustomExceptionHandler` provides centralized exception handling for your eShop modular monolith, converting exceptions into structured JSON responses with appropriate HTTP status codes.

## ? **Features Implemented**

### **?? Exception Types Handled**
- **ValidationException** (FluentValidation) ? 400 Bad Request with field-specific errors
- **BadRequestException** (Custom) ? 400 Bad Request
- **NotFoundException** (Custom) ? 404 Not Found  
- **InternalServerException** (Custom) ? 500 Internal Server Error
- **Generic Exceptions** ? 500 Internal Server Error

### **?? Response Structure**

#### **Standard Error Response**
```json
{
  "title": "Bad Request",
  "detail": "Product name is required",
  "statusCode": 400,
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

#### **Validation Error Response**
```json
{
  "title": "Validation Failed",
  "detail": "One or more validation errors occurred",
  "statusCode": 400,
  "timestamp": "2024-01-15T10:30:00.000Z",
  "errors": {
    "Product.Name": ["Product name is required", "Product name cannot exceed 200 characters"],
    "Product.Price": ["Product price must be greater than 0"]
  }
}
```

## ?? **Usage Examples**

### **1. In Command Handlers**

```csharp
public class CreateProductHandler : ICommandHandler<CreateProductCommand, CreateProductResult>
{
    public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        // Validation exceptions are automatically thrown by ValidationBehavior
        // Custom exceptions can be thrown manually:
        
        var existingProduct = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Name == command.Product.Name, cancellationToken);
            
        if (existingProduct != null)
        {
            throw ProductExceptions.DuplicateProduct(command.Product.Name);
        }

        if (command.Product.Price <= 0)
        {
            throw ProductExceptions.InvalidProductPrice(command.Product.Price);
        }

        // ... rest of implementation
    }
}
```

### **2. In Query Handlers**

```csharp
public class GetProductHandler : IQueryHandler<GetProductQuery, ProductDto?>
{
    public async Task<ProductDto?> Handle(GetProductQuery query, CancellationToken cancellationToken)
    {
        var product = await _mappingService
            .ProjectToType<ProductDto>(_dbContext.Products.Where(p => p.Id == query.ProductId))
            .FirstOrDefaultAsync(cancellationToken);

        if (product == null)
        {
            throw ProductExceptions.ProductNotFound(query.ProductId);
        }

        return product;
    }
}
```

### **3. In Carter Endpoints**

```csharp
public class GetProductEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/product/{productId:guid}", async (Guid productId, ISender sender) =>
        {
            // Exceptions thrown in handlers are automatically caught by CustomExceptionHandler
            var query = new GetProductQuery(productId);
            var result = await sender.Send(query);
            
            return Results.Ok(new GetProductResponse(result));
        });
    }
}
```

## ?? **Setup & Configuration**

### **1. Registration in Program.cs**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register exception handler
builder.Services.AddCustomExceptionHandler();

var app = builder.Build();

// Add exception handling middleware (MUST be early in pipeline)
app.UseExceptionHandler();

// Other middleware...
app.Run();
```

### **2. Exception Factory Classes**

Use factory methods for consistent exception creation:

```csharp
// Instead of: throw new NotFoundException("Product not found");
// Use: throw ProductExceptions.ProductNotFound(productId);
```

### **3. FluentValidation Integration**

The handler automatically converts `ValidationException` to structured responses:

```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Product.Name)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(200)
            .WithMessage("Product name cannot exceed 200 characters");
            
        RuleFor(x => x.Product.Price)
            .GreaterThan(0)
            .WithMessage("Product price must be greater than 0");
    }
}
```

## ?? **Best Practices**

### **? Do:**
- Use specific exception factory methods
- Provide meaningful error messages
- Include relevant details in exception messages
- Log exceptions for debugging
- Use appropriate HTTP status codes

### **? Don't:**
- Create exceptions directly in business logic
- Include sensitive information in error messages
- Use generic exception messages
- Forget to register the exception handler

## ?? **Testing Exception Handling**

### **Unit Test Example**

```csharp
[Test]
public async Task CreateProduct_WithInvalidPrice_ThrowsBadRequestException()
{
    // Arrange
    var command = new CreateProductCommand(new ProductDto
    {
        Name = "Test Product",
        Price = -10.00m // Invalid price
    });

    // Act & Assert
    var exception = await Assert.ThrowsAsync<BadRequestException>(
        () => _handler.Handle(command, CancellationToken.None));
        
    Assert.That(exception.Message, Is.EqualTo("Invalid product price"));
}
```

### **Integration Test Example**

```csharp
[Test]
public async Task GetProduct_WithInvalidId_Returns404()
{
    // Arrange
    var invalidId = Guid.NewGuid();
    
    // Act
    var response = await _client.GetAsync($"/product/{invalidId}");
    
    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    
    var content = await response.Content.ReadAsStringAsync();
    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content);
    
    Assert.That(errorResponse.Title, Is.EqualTo("Not Found"));
    Assert.That(errorResponse.StatusCode, Is.EqualTo(404));
}
```

## ?? **Benefits Achieved**

- **?? Consistent Error Responses** - All errors follow the same JSON structure
- **?? Better Debugging** - Structured logging with timestamps and details
- **??? Security** - Controlled error information exposure
- **?? Testability** - Easy to test exception scenarios
- **?? Developer Experience** - Clear error messages for API consumers
- **?? Centralized Handling** - Single place to modify error response format

## ?? **Error Response HTTP Status Codes**

| Exception Type | Status Code | When Used |
|----------------|-------------|-----------|
| `ValidationException` | 400 | FluentValidation failures |
| `BadRequestException` | 400 | Invalid request data |
| `NotFoundException` | 404 | Resource not found |
| `InternalServerException` | 500 | Server-side errors |
| `Generic Exception` | 500 | Unhandled exceptions |

Your eShop modular monolith now has **enterprise-grade exception handling** that provides consistent, secure, and developer-friendly error responses! ??
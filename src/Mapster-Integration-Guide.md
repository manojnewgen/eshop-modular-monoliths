# ?? Mapster Integration Guide - Object Mapping Excellence

## ?? **Overview**

Mapster has been successfully integrated into your eShop modular monolith, providing high-performance object mapping capabilities that work seamlessly with your CQRS + MediatR + SaveChanges interceptor architecture.

## ?? **Why Mapster?**

### **? Performance Benefits**
- **Compiled Expressions** - Generates fast compiled code, not reflection
- **Database Projection** - Direct SQL projection with `ProjectToType()`
- **Memory Efficient** - Minimal object allocation
- **Zero Configuration** - Works with conventions out of the box

### **? Developer Experience**
- **Type Safety** - Compile-time checking of mappings
- **IntelliSense Support** - Full IDE support with auto-completion
- **Fluent Configuration** - Easy to configure complex mappings
- **Debugging Friendly** - Clear stack traces and error messages

## ??? **Architecture Integration**

### **?? File Structure**
```
Modules/
??? Shared/Shared/Mapping/
?   ??? IMappingConfiguration.cs     # Base interface for mapping configs
?   ??? MappingService.cs           # DI-friendly mapping service
??? Catalog/Catalog/Products/
    ??? Mappings/
        ??? ProductMappingConfiguration.cs  # Product-specific mappings
```

### **?? Service Registration**
```csharp
// In CatalogModule.cs
services.AddMapster(Assembly.GetExecutingAssembly());

// This automatically:
// ? Finds all IMappingConfiguration implementations
// ? Configures TypeAdapterConfig
// ? Registers IMappingService for DI
// ? Compiles configurations for performance
```

## ?? **Mapping Configurations**

### **?? Product Mapping Configuration**

```csharp
public class ProductMappingConfiguration : IMappingConfiguration
{
    public void Configure(TypeAdapterConfig config)
    {
        // 1. Product ? ProductDto (Query scenarios)
        config.NewConfig<Product, ProductDto>()
            .Map(dest => dest.Categories, src => src.Categories.ToList());

        // 2. ProductDto ? Product (Create scenarios)
        config.NewConfig<ProductDto, Product>()
            .ConstructUsing(src => Product.Create(
                src.Id == Guid.Empty ? Guid.NewGuid() : src.Id,
                src.Name, src.Description, src.Price, src.ImageFile,
                src.Categories, src.StockQuantity));

        // 3. ProductDto ? Product (Update scenarios)
        config.NewConfig<ProductDto, Product>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Events)
            .AfterMapping((src, dest) => {
                // Smart domain method calls
                if (dest.Price != src.Price)
                    dest.UpdatePrice(src.Price, "Updated via API");
                // ... other domain method calls
            });
    }
}
```

## ?? **Handler Integration Examples**

### **?? Query Handlers (Database Projection)**

```csharp
public class GetProductHandler : IQueryHandler<GetProductQuery, ProductDto?>
{
    private readonly IMappingService _mappingService;

    public async Task<ProductDto?> Handle(GetProductQuery query, CancellationToken cancellationToken)
    {
        // ? Database projection - SQL is generated with only needed fields
        return await _mappingService
            .ProjectToType<ProductDto>(_dbContext.Products.Where(p => p.Id == query.ProductId))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
```

### **?? Command Handlers (Domain Mapping)**

```csharp
public class CreateProductHandler : ICommandHandler<CreateProductCommand, CreateProductResult>
{
    private readonly IMappingService _mappingService;

    public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        // ? Smart mapping - Uses Product.Create() factory method
        var product = _mappingService.Map<ProductDto, Product>(command.Product);
        
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return new CreateProductResult(product.Id);
    }
}

public class UpdateProductHandler : ICommandHandler<UpdateProductCommand, UpdateProductResult>
{
    private readonly IMappingService _mappingService;

    public async Task<UpdateProductResult> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var existingProduct = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == command.ProductId, cancellationToken);

        // ? Smart update - Calls domain methods automatically
        _mappingService.Map(command.Product, existingProduct);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return new UpdateProductResult(command.ProductId, true);
    }
}
```

## ?? **Usage Patterns**

### **1. Direct Mapping Service Usage**

```csharp
public class ProductService
{
    private readonly IMappingService _mappingService;

    // Generic mapping
    var dto = _mappingService.Map<ProductDto>(product);

    // Typed mapping
    var dto = _mappingService.Map<Product, ProductDto>(product);

    // Map to existing object
    _mappingService.Map(productDto, existingProduct);

    // Database projection
    var dtos = await _mappingService
        .ProjectToType<ProductDto>(dbContext.Products)
        .ToListAsync();
}
```

### **2. Handler Integration**

```csharp
// Inject IMappingService in any handler
public SomeHandler(IMappingService mappingService)
{
    _mappingService = mappingService;
}
```

## ? **Performance Benefits**

### **?? Before vs After Comparison**

| Aspect | Before Mapster | After Mapster |
|--------|----------------|---------------|
| **Query Handlers** | Manual `Select()` projections | Automatic `ProjectToType()` |
| **Command Handlers** | Manual property mapping | Configured smart mapping |
| **Performance** | Reflection-based | Compiled expressions |
| **SQL Generation** | Select all fields | Select only needed fields |
| **Memory Usage** | High allocation | Minimal allocation |
| **Type Safety** | Runtime errors possible | Compile-time checking |
| **Maintainability** | Repetitive code | Centralized configuration |

### **?? Database Projection Example**

```sql
-- Before Mapster (Select all fields)
SELECT Id, Name, Description, Price, ImageFile, Categories, StockQuantity, 
       IsAvailable, IsDeleted, CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy,
       DeletedAt, DeletedBy
FROM catalog.products

-- After Mapster ProjectToType<ProductDto> (Select only DTO fields)
SELECT Id, Name, Description, Price, ImageFile, Categories, StockQuantity
FROM catalog.products
```

## ?? **Advanced Features**

### **1. Custom Type Adapters**

```csharp
config.NewConfig<Product, ProductDto>()
    .Map(dest => dest.Categories, src => src.Categories.ToList())
    .Map(dest => dest.Price, src => Math.Round(src.Price, 2));
```

### **2. Conditional Mapping**

```csharp
config.NewConfig<ProductDto, Product>()
    .Map(dest => dest.Name, src => src.Name, srcCond => !string.IsNullOrEmpty(src.Name));
```

### **3. Nested Object Mapping**

```csharp
config.NewConfig<OrderDto, Order>()
    .Map(dest => dest.Items, src => src.Items); // Automatically maps OrderItemDto[] to OrderItem[]
```

### **4. Collection Handling**

```csharp
config.NewConfig<Product, ProductDto>()
    .Map(dest => dest.Categories, src => src.Categories.ToList())
    .PreserveReference(true); // Maintain object references
```

## ?? **Testing with Mapster**

### **Unit Testing Mappings**

```csharp
[Test]
public void Should_Map_Product_To_ProductDto()
{
    // Arrange
    var config = TypeAdapterConfig.GlobalSettings;
    new ProductMappingConfiguration().Configure(config);
    
    var product = Product.Create(Guid.NewGuid(), "Test", "Description", 99.99m, "test.jpg");
    
    // Act
    var dto = product.Adapt<ProductDto>(config);
    
    // Assert
    Assert.That(dto.Name, Is.EqualTo("Test"));
    Assert.That(dto.Price, Is.EqualTo(99.99m));
}
```

## ?? **Best Practices**

### **? Do's**
- ? **Use ProjectToType()** for database queries to minimize data transfer
- ? **Configure mappings** in dedicated configuration classes
- ? **Use domain methods** in AfterMapping for business logic
- ? **Test your mappings** with unit tests
- ? **Leverage IMappingService** for dependency injection

### **? Don'ts**
- ? **Don't use .Adapt()** directly in handlers - use IMappingService
- ? **Don't ignore mapping configurations** - they provide business logic
- ? **Don't map audit fields** in update scenarios - let interceptor handle them
- ? **Don't skip validation** - domain methods provide validation

## ?? **Integration Points**

### **?? CQRS Pattern**
- **Queries** ? Use `ProjectToType()` for efficient database projections
- **Commands** ? Use `Map()` for DTO ? Domain entity conversion

### **? MediatR Integration**
- **Handlers** ? Inject `IMappingService` for consistent mapping
- **Results** ? Map domain entities back to DTOs for responses

### **?? SaveChanges Interceptor**
- **Audit Fields** ? Automatically handled, no mapping needed
- **Domain Events** ? Raised by domain methods called in mapping
- **Soft Deletes** ? Handled by interceptor, transparent to mapping

## ?? **Summary**

Your eShop modular monolith now has **enterprise-grade object mapping** with:

- ? **High Performance** - Compiled expressions and database projection
- ? **Type Safety** - Compile-time checking and IntelliSense support  
- ? **Clean Architecture** - Proper separation between DTOs and domain entities
- ? **Domain Integration** - Smart mapping that calls domain methods
- ? **CQRS Optimization** - Efficient query projections and command mapping
- ? **Maintainable Code** - Centralized, configured mapping logic

Mapster seamlessly integrates with your existing CQRS + MediatR + SaveChanges interceptor architecture, providing optimal performance while maintaining clean, testable code! ??
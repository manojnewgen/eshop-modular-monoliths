# ?? Domain Event Handlers with MediatR INotification - Complete Guide

## ?? **Overview**

This guide demonstrates a comprehensive implementation of domain event handlers using MediatR's `INotification` pattern in your eShop modular monolith. The implementation showcases enterprise-grade event-driven architecture with loose coupling, scalability, and maintainability.

## ??? **Architecture Foundation**

### **Base Domain Event Interface**
```csharp
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}

public abstract record BaseDomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => GetType().AssemblyQualifiedName!;
}
```

### **Event Flow Architecture**
```
Domain Operation ? Domain Events Raised ? SaveChanges Interceptor ? MediatR Dispatch ? Event Handlers
```

## ?? **Event Handler Categories**

### **1. ?? Search & Indexing Handlers**

**`ProductSearchIndexHandler`** - Maintains search indexes
- **Events:** ProductCreated, ProductPriceChanged, ProductCategoriesUpdated, ProductDeleted, ProductRestored
- **Purpose:** Keep search engines (Elasticsearch, Azure Search) synchronized
- **Benefits:** Real-time search results, better customer experience

```csharp
public class ProductSearchIndexHandler : 
    INotificationHandler<ProductCreatedEvent>,
    INotificationHandler<ProductPriceChangedEvent>
{
    public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Update search index with new product
        await UpdateSearchIndex(notification.ProductId, "CREATE");
    }
}
```

### **2. ?? Inventory Management Handlers**

**`ProductInventoryHandler`** - Inventory lifecycle management
- **Events:** ProductCreated, ProductDeleted, ProductRestored
- **Purpose:** Initialize, suspend, and reactivate inventory tracking
- **Benefits:** Automated inventory management, stock level monitoring

**`ProductStockAlertHandler`** - Low stock notifications
- **Events:** ProductCreated (to set up monitoring)
- **Purpose:** Monitor stock levels and send alerts
- **Benefits:** Prevent stockouts, optimize inventory levels

### **3. ?? Analytics & Business Intelligence Handlers**

**`ProductPricingAnalyticsHandler`** - Price change analytics
- **Events:** ProductPriceChanged
- **Purpose:** Track pricing trends, calculate metrics, detect significant changes
- **Benefits:** Data-driven pricing decisions, competitive analysis

**`ProductBusinessIntelligenceHandler`** - BI metrics collection
- **Events:** ProductCreated, ProductPriceChanged, ProductCategoriesUpdated
- **Purpose:** Feed data to BI systems (Power BI, Tableau)
- **Benefits:** Business insights, reporting, KPI tracking

### **4. ?? Notification & Communication Handlers**

**`ProductNotificationHandler`** - Customer notifications
- **Events:** ProductCreated, ProductPriceChanged, ProductDeleted, ProductRestored
- **Purpose:** Notify customers, update wishlists, send alerts
- **Benefits:** Enhanced customer engagement, personalized experience

### **5. ?? Integration & External System Handlers**

**`ProductIntegrationHandler`** - External system synchronization
- **Events:** ProductCreated, ProductPriceChanged, ProductDeleted
- **Purpose:** Publish integration events, sync with external catalogs
- **Benefits:** System interoperability, data consistency across platforms

### **6. ?? Cross-Module Communication Handlers**

**`ProductEventForBasketHandler`** - Basket module integration
- **Events:** ProductCreated, ProductPriceChanged, ProductDeleted
- **Purpose:** Update basket contents, recalculate totals, handle unavailable products
- **Benefits:** Loose coupling between modules, consistent pricing

**`ProductEventForOrderingHandler`** - Ordering module integration
- **Events:** ProductCreated, ProductDeleted
- **Purpose:** Enable/disable products for ordering, handle fulfillment rules
- **Benefits:** Order integrity, proper product lifecycle management

## ?? **Key Implementation Patterns**

### **1. Single Responsibility Handlers**
```csharp
// ? Good - Single purpose
public class ProductSearchIndexHandler : INotificationHandler<ProductCreatedEvent>

// ? Avoid - Multiple unrelated responsibilities
public class ProductEverythingHandler : INotificationHandler<ProductCreatedEvent>
```

### **2. Multiple Event Handler**
```csharp
public class ProductAnalyticsHandler : 
    INotificationHandler<ProductCreatedEvent>,
    INotificationHandler<ProductPriceChangedEvent>,
    INotificationHandler<ProductCategoriesUpdatedEvent>
{
    // Handle related events that serve the same business purpose
}
```

### **3. Error Handling and Resilience**
```csharp
public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
{
    try
    {
        await ProcessEvent(notification);
        _logger.LogInformation("Successfully processed event: {EventId}", notification.EventId);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to process event: {EventId}", notification.EventId);
        // Don't throw - this is a side effect, not critical to main operation
        // Consider: Dead letter queue, retry logic, alerting
    }
}
```

### **4. Async Best Practices**
```csharp
public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
{
    // ? Use ConfigureAwait(false) for library code
    await SomeAsyncOperation().ConfigureAwait(false);
    
    // ? Pass cancellation token through
    await AnotherOperation(cancellationToken);
    
    // ? Handle timeouts appropriately
    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    timeoutCts.CancelAfter(TimeSpan.FromSeconds(30));
}
```

## ?? **Event Handler Registration**

### **Automatic Registration via MediatR**
```csharp
// In CatalogModule.cs
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// This automatically registers all INotificationHandler<T> implementations
```

### **Handler Discovery**
MediatR automatically finds and registers:
- ? `ProductSearchIndexHandler`
- ? `ProductInventoryHandler` 
- ? `ProductAnalyticsHandler`
- ? `ProductNotificationHandler`
- ? `ProductIntegrationHandler`
- ? All other `INotificationHandler<IDomainEvent>` implementations

## ?? **Event Dispatch Flow**

### **1. Domain Operation Triggers Event**
```csharp
public void UpdatePrice(decimal newPrice, string reason)
{
    var oldPrice = Price;
    Price = newPrice;
    
    // Raise domain event
    AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice, newPrice, reason));
}
```

### **2. SaveChanges Interceptor Collects Events**
```csharp
// In SaveChangesInterceptor
private void CollectDomainEvents(EntityEntry entry)
{
    if (entry.Entity is IAggregate aggregate && aggregate.Events.Any())
    {
        _domainEvents.AddRange(aggregate.Events);
        aggregate.ClearDomainEvents();
    }
}
```

### **3. Events Dispatched After Successful Save**
```csharp
// In SaveChangesInterceptor
public async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken)
{
    if (eventData.Context != null)
    {
        await DispatchDomainEventsAsync(); // All handlers called here
    }
    return await base.SavedChangesAsync(eventData, result, cancellationToken);
}
```

### **4. MediatR Calls All Registered Handlers**
```csharp
await mediator.Publish(domainEvent); // Calls ALL INotificationHandler<ProductPriceChangedEvent>
```

## ?? **Benefits Demonstrated**

### **? Loose Coupling**
- Modules communicate without direct dependencies
- Easy to add new handlers without changing existing code
- Domain logic remains clean and focused

### **? Scalability**
- Multiple handlers can process the same event
- Handlers can be added or removed independently
- Easy to implement retry logic and error handling

### **? Auditability**
- Every domain event has EventId and OccurredOn
- Complete audit trail of all business operations
- Easy to replay events for debugging

### **? Eventual Consistency**
- Side effects processed asynchronously
- System remains responsive during peak loads
- Graceful degradation if handlers fail

### **? Extensibility**
- New business requirements = new event handlers
- No changes to core domain logic required
- Easy to A/B test new features

## ?? **Testing Event Handlers**

### **Unit Testing Individual Handlers**
```csharp
[Test]
public async Task ProductSearchIndexHandler_Should_Update_Index_On_Product_Created()
{
    // Arrange
    var mockLogger = Mock.Of<ILogger<ProductSearchIndexHandler>>();
    var mockDbContext = CreateMockDbContext();
    var handler = new ProductSearchIndexHandler(mockLogger, mockDbContext);
    
    var productCreatedEvent = new ProductCreatedEvent(
        ProductId: Guid.NewGuid(),
        Name: "Test Product",
        Price: 99.99m,
        Categories: new List<string> { "Test" }
    );

    // Act
    await handler.Handle(productCreatedEvent, CancellationToken.None);

    // Assert
    // Verify expected behavior (logs, external service calls, etc.)
}
```

### **Integration Testing Event Dispatch**
```csharp
[Test]
public async Task Should_Dispatch_Events_To_All_Handlers_After_SaveChanges()
{
    // Arrange
    var product = Product.Create(...);
    _dbContext.Products.Add(product);

    // Act
    await _dbContext.SaveChangesAsync(); // This should trigger event handlers

    // Assert
    // Verify all expected handlers were called
    // Check logs, database state, external service calls
}
```

## ?? **File Organization**

```
Modules/Catalog/Catalog/Products/Handlers/DomainEvents/
??? ProductSearchIndexHandler.cs          # Search & indexing
??? ProductInventoryHandler.cs            # Inventory management  
??? ProductAnalyticsHandler.cs            # Analytics & BI
??? ProductNotificationHandler.cs         # Customer notifications
??? CrossModuleEventHandlers.cs          # Cross-module communication
??? ProductDomainEventHandlers.cs        # Basic event logging
```

## ?? **Summary**

Your eShop modular monolith now has **enterprise-grade domain event handling** that provides:

### **?? Business Value**
- **Real-time updates** across all systems
- **Personalized customer experiences** through notifications
- **Data-driven insights** through analytics
- **Operational efficiency** through automation

### **??? Technical Excellence**
- **Clean Architecture** with proper separation of concerns
- **Loose Coupling** between modules and systems
- **High Scalability** with async event processing
- **Strong Auditability** with complete event trails

### **?? Development Benefits**
- **Easy to extend** with new business requirements
- **Simple to test** with isolated handler logic
- **Fast to implement** new features via event handlers
- **Maintainable code** with single responsibility handlers

The implementation demonstrates how domain events with MediatR's `INotification` pattern enable building sophisticated, scalable systems while maintaining clean, testable code! ??
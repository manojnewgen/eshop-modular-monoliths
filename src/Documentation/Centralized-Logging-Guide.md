# ?? Centralized Logging with MediatR Pipeline Behaviors

## ?? **Overview**

This implementation provides comprehensive centralized logging for your eShop modular monolith using MediatR Pipeline Behaviors. All requests and responses flowing through your CQRS handlers are automatically logged with structured data, performance metrics, and contextual information.

## ?? **Features Implemented**

### **?? LoggingBehavior**
- **Request/Response Logging** - Automatic logging of all commands and queries
- **Execution Time Tracking** - Measures and logs request duration
- **Structured Logging** - JSON serialization with configurable detail levels
- **Performance Warnings** - Automatic alerts for slow requests
- **Exception Logging** - Detailed error logging with context
- **Distributed Tracing** - Activity/OpenTelemetry integration

### **? PerformanceBehavior**
- **Execution Time Monitoring** - Tracks request duration
- **Memory Usage Tracking** - Monitors memory allocation per request
- **Performance Thresholds** - Configurable alerts for slow/resource-heavy requests
- **Metrics Collection** - Ready for Application Insights/Prometheus integration

### **?? RequestContextBehavior**
- **Correlation ID Management** - Automatic correlation ID generation and propagation
- **User Context Tracking** - Captures user information from HTTP context
- **Client Information** - Logs IP address, User-Agent, and request headers
- **Distributed Tracing Tags** - Adds context to OpenTelemetry activities
- **Scoped Logging** - Creates logging scopes with contextual information

### **? ValidationBehavior** (Enhanced)
- **Automatic Validation** - Validates requests using FluentValidation
- **Detailed Error Reporting** - Field-level validation errors
- **Performance Optimized** - Only runs when validators are registered

## ??? **Pipeline Architecture**

### **Execution Order (Inside ? Outside)**
```
1. ?? RequestContextBehavior    (Outermost - establishes context)
2. ?? LoggingBehavior          (Logs everything with context)
3. ? PerformanceBehavior       (Tracks metrics)
4. ? ValidationBehavior       (Validates input)
5. ?? Your Handler             (Business logic)
```

### **Request Flow**
```
HTTP Request
    ?
?? Correlation ID + User Context
    ?
?? Log: "Starting CreateProduct [abc123]"
    ?
? Start: Performance tracking
    ?
? Validate: Product data
    ?
?? Execute: CreateProductHandler
    ?
? Stop: Performance tracking
    ?
?? Log: "Completed CreateProduct [abc123] in 250ms"
    ?
HTTP Response (with X-Correlation-ID header)
```

## ?? **Log Output Examples**

### **Request Start Log**
```json
{
  "timestamp": "2024-01-15T10:30:00.123Z",
  "level": "Information",
  "message": "?? Starting request CreateProductCommand [abc123] at 2024-01-15T10:30:00.123Z",
  "properties": {
    "RequestName": "CreateProductCommand",
    "RequestId": "abc123",
    "CorrelationId": "abc123",
    "UserId": "user-456",
    "IpAddress": "192.168.1.100"
  }
}
```

### **Request Completion Log**
```json
{
  "timestamp": "2024-01-15T10:30:00.373Z",
  "level": "Information", 
  "message": "? Completed request CreateProductCommand [abc123] in 250ms",
  "properties": {
    "RequestName": "CreateProductCommand",
    "RequestId": "abc123",
    "ElapsedMs": 250,
    "CorrelationId": "abc123",
    "MemoryUsed": 1048576
  }
}
```

### **Performance Warning Log**
```json
{
  "timestamp": "2024-01-15T10:30:02.500Z",
  "level": "Warning",
  "message": "?? Slow request detected: GetProductsQuery [def456] took 2500ms",
  "properties": {
    "RequestName": "GetProductsQuery",
    "RequestId": "def456", 
    "ElapsedMs": 2500,
    "MemoryUsed": 5242880
  }
}
```

### **Validation Error Log**
```json
{
  "timestamp": "2024-01-15T10:30:00.050Z",
  "level": "Warning",
  "message": "Validation failed for CreateProductCommand with 2 errors",
  "properties": {
    "RequestName": "CreateProductCommand",
    "ValidationErrors": {
      "Product.Name": ["Product name is required"],
      "Product.Price": ["Product price must be greater than 0"]
    }
  }
}
```

## ?? **Setup & Registration**

### **1. Module Registration**

Your `CatalogModule` is already updated:

```csharp
public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
{
    // Register MediatR with all pipeline behaviors
    services.AddMediatRWithBehaviors(Assembly.GetExecutingAssembly());
    
    return services;
}
```

### **2. Alternative Registration Options**

```csharp
// Option 1: Register all behaviors (recommended)
services.AddMediatRWithBehaviors(typeof(CatalogModule).Assembly);

// Option 2: Register only logging and performance (if you handle validation separately)
services.AddLoggingAndPerformanceBehaviors();

// Option 3: Register behaviors individually
services.AddPipelineBehaviors();
```

### **3. Configure Logging Levels**

**appsettings.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Shared.Behaviors.LoggingBehavior": "Information",
      "Shared.Behaviors.PerformanceBehavior": "Information",
      "Shared.Behaviors.RequestContextBehavior": "Debug"
    }
  }
}
```

**appsettings.Development.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Shared.Behaviors": "Debug"
    }
  }
}
```

## ?? **Monitoring & Observability**

### **Structured Logging Properties**

Every log entry includes these structured properties:

| Property | Description | Example |
|----------|-------------|---------|
| `RequestName` | Type of request | "CreateProductCommand" |
| `RequestId` | Unique request identifier | "abc123" |
| `CorrelationId` | Cross-request correlation | "abc123" |
| `UserId` | Current user (if authenticated) | "user-456" |
| `IpAddress` | Client IP address | "192.168.1.100" |
| `ElapsedMs` | Request duration | 250 |
| `MemoryUsed` | Memory allocation | 1048576 |

### **OpenTelemetry Integration**

The behaviors automatically add telemetry tags:

```csharp
activity?.SetTag("request.name", requestName);
activity?.SetTag("request.id", requestId);
activity?.SetTag("request.duration_ms", elapsedMs);
activity?.SetTag("request.success", exception == null);
activity?.SetTag("correlation.id", correlationId);
activity?.SetTag("user.id", userId);
```

### **Application Insights Integration**

To send custom metrics to Application Insights:

```csharp
// In PerformanceBehavior, uncomment this line:
// _telemetryClient.GetMetric("request.performance").TrackValue(elapsedMs, tags);
```

## ?? **Testing with Logging**

### **Unit Test Example**

```csharp
[Test]
public async Task CreateProduct_ShouldLogRequestAndResponse()
{
    // Arrange
    var logger = new Mock<ILogger<LoggingBehavior<CreateProductCommand, CreateProductResult>>>();
    var behavior = new LoggingBehavior<CreateProductCommand, CreateProductResult>(logger.Object);
    
    var command = new CreateProductCommand(new ProductDto { Name = "Test Product" });
    
    // Act
    var result = await behavior.Handle(command, () => Task.FromResult(new CreateProductResult(Guid.NewGuid())), CancellationToken.None);
    
    // Assert
    logger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting request CreateProductCommand")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
}
```

### **Integration Test Example**

```csharp
[Test]
public async Task CreateProduct_ShouldHaveCorrelationIdInResponse()
{
    // Arrange
    var request = new HttpRequestMessage(HttpMethod.Post, "/product");
    request.Headers.Add("X-Correlation-ID", "test-123");
    
    // Act
    var response = await _client.SendAsync(request);
    
    // Assert
    Assert.That(response.Headers.GetValues("X-Correlation-ID").First(), Is.EqualTo("test-123"));
}
```

## ?? **Best Practices**

### **? Do:**
- Use structured logging properties instead of string interpolation
- Configure appropriate log levels for production vs development
- Include correlation IDs in external service calls
- Monitor performance metrics and set up alerts
- Use log scopes for contextual information

### **? Don't:**
- Log sensitive information (passwords, tokens, PII)
- Log large objects in production (configure `IsLoggableResponse`)
- Set debug logging in production
- Ignore performance warnings
- Log business logic details in pipeline behaviors

## ?? **Configuration Options**

### **Performance Thresholds**

Modify these values in `PerformanceBehavior.cs`:

```csharp
const long SlowRequestThreshold = 1000; // 1 second
const long VerySlowRequestThreshold = 5000; // 5 seconds  
const long HighMemoryUsage = 1024 * 1024; // 1 MB
```

### **Logging Exclusions**

Configure which responses to exclude from detailed logging in `LoggingBehavior.cs`:

```csharp
private static bool IsLoggableResponse(Type responseType)
{
    var nonLoggableTypes = new[] { "Stream", "FileResult", "byte[]" };
    return !nonLoggableTypes.Any(t => responseType.Name.Contains(t));
}
```

## ?? **Metrics Dashboard**

Use these queries with your logging provider:

### **Request Volume (Last 24h)**
```kql
// Application Insights
requests
| where timestamp > ago(24h)
| summarize count() by bin(timestamp, 1h), name
```

### **Slow Requests**
```kql
traces  
| where message contains "Slow request"
| project timestamp, message, customDimensions.RequestName, customDimensions.ElapsedMs
```

### **Error Rate**
```kql
traces
| where message contains "Failed request"
| summarize ErrorCount=count() by bin(timestamp, 5m)
```

## ?? **Benefits Achieved**

- **?? Complete Observability** - Every request is automatically logged
- **?? Performance Monitoring** - Identify bottlenecks and optimize
- **?? Request Tracing** - Follow requests across distributed systems  
- **??? Security Auditing** - Track user actions and access patterns
- **?? Better Debugging** - Rich context for troubleshooting issues
- **?? Business Intelligence** - Analyze usage patterns and performance
- **? Zero Code Changes** - Automatic logging for all handlers
- **?? Structured Data** - Query logs efficiently with structured properties

Your eShop modular monolith now has **enterprise-grade centralized logging** that provides comprehensive observability without any changes to your existing handlers! ??
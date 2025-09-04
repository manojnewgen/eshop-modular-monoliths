# ??? Generic Database Migration Extensions

This document explains how to use the generic database migration extensions in the eShop Modular Monolith application.

## ?? Overview

The migration extensions provide a consistent, robust way to handle Entity Framework Core migrations across all modules in the modular monolith architecture. They include retry logic, error handling, and support for multiple migration strategies.

## ??? Architecture

### Core Components

1. **`MigrationExtensions`** - Core generic migration methods
2. **`ModuleMigrationExtensions`** - Auto-discovery and bulk migration methods
3. **`MigrationInfo`** - Information about migration status

### Key Features

- ? **Generic approach** - Works with any `DbContext`
- ? **Retry logic** - Configurable retry attempts with delays
- ? **Error handling** - Graceful handling of migration failures
- ? **Auto-discovery** - Automatically finds all registered DbContexts
- ? **Environment-aware** - Different behavior for dev vs production
- ? **Monitoring** - Built-in endpoints for migration status
- ? **Async support** - Both sync and async methods available

## ?? Usage Examples

### 1. Basic Usage - Single DbContext

```csharp
// In your module (e.g., CatalogModule.cs)
public static IApplicationBuilder UseCatalogModule(this IApplicationBuilder app)
{
    // Apply migrations with default settings (3 retries, 5-second delay)
    app.UseMigration<CatalogDbContext>();
    
    return app;
}
```

### 2. Advanced Usage - Custom Retry Settings

```csharp
// Apply migrations with custom retry settings
await app.UseMigrationAsync<CatalogDbContext>(
    retryCount: 5,        // 5 retry attempts
    retryDelay: 3000      // 3-second delay between retries
);
```

### 3. Auto-Discovery - All Modules

```csharp
// In Program.cs - automatically discovers and migrates all registered DbContexts
app.UseModuleMigrations();

// Or async version with custom settings
await app.UseModuleMigrationsAsync(retryCount: 5, retryDelay: 3000);
```

### 4. Multiple Specific DbContexts

```csharp
// Migrate specific DbContext types
await app.UseMigrationsAsync(new[]
{
    typeof(CatalogDbContext),
    typeof(BasketDbContext),
    typeof(OrderingDbContext)
});
```

### 5. Migration Information

```csharp
// Get migration info for a specific DbContext
var catalogInfo = await app.GetMigrationInfoAsync<CatalogDbContext>();
Console.WriteLine($"Catalog: {catalogInfo}");

// Get migration info for all registered DbContexts
var allInfo = await app.GetAllMigrationInfoAsync();
foreach (var info in allInfo)
{
    Console.WriteLine($"{info.ContextName}: Applied={info.AppliedMigrations.Count}, Pending={info.PendingMigrations.Count}");
}
```

## ??? Module Integration

### Step 1: Register DbContext in Module

```csharp
public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<CatalogDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("CatalogConnection");
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "catalog");
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
            });
        });

        return services;
    }

    public static IApplicationBuilder UseCatalogModule(this IApplicationBuilder app)
    {
        // Apply migrations
        app.UseMigration<CatalogDbContext>();
        
        return app;
    }
}
```

### Step 2: Use in Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register modules
builder.Services.AddCatalogModule(builder.Configuration)
                .AddBasketModule(builder.Configuration);

var app = builder.Build();

// Option A: Let modules handle their own migrations
app.UseCatalogModule()
   .UseBasketModule();

// Option B: Use auto-discovery
// app.UseModuleMigrations();

app.Run();
```

## ?? Monitoring & Health Checks

### Migration Status Endpoint

```csharp
// Add migration status endpoint
app.MapMigrationStatusEndpoint("/migration-status");

// Custom endpoint path
app.MapMigrationStatusEndpoint("/admin/migrations");
```

Example response:
```json
{
  "timestamp": "2024-01-15T10:30:00Z",
  "status": "Healthy",
  "contexts": [
    {
      "contextName": "CatalogDbContext",
      "canConnect": true,
      "hasPendingMigrations": false,
      "appliedMigrationsCount": 3,
      "pendingMigrationsCount": 0,
      "error": null
    },
    {
      "contextName": "BasketDbContext",
      "canConnect": true,
      "hasPendingMigrations": true,
      "appliedMigrationsCount": 2,
      "pendingMigrationsCount": 1,
      "pendingMigrations": ["20240115_AddNewField"]
    }
  ]
}
```

### Custom Migration Info Endpoint

```csharp
app.MapGet("/migration-info", async () =>
{
    var migrationInfos = await app.GetAllMigrationInfoAsync();
    return Results.Ok(new
    {
        Timestamp = DateTime.UtcNow,
        Migrations = migrationInfos
    });
});
```

## ?? Configuration Options

### Retry Configuration

| Parameter | Default | Description |
|-----------|---------|-------------|
| `retryCount` | 3 | Number of retry attempts |
| `retryDelay` | 5000ms | Delay between retries |

### Environment Behavior

| Environment | Behavior |
|-------------|----------|
| **Development** | - Detailed error logging<br>- Sensitive data logging enabled<br>- Exceptions may be thrown |
| **Production** | - Basic error logging<br>- Graceful failure handling<br>- Application continues startup |

## ?? Best Practices

### 1. Module Design

```csharp
public static class YourModule
{
    public static IServiceCollection AddYourModule(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register DbContext with proper configuration
        services.AddDbContext<YourDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "your_schema");
                npgsql.EnableRetryOnFailure();
            });
        });

        return services;
    }

    public static IApplicationBuilder UseYourModule(this IApplicationBuilder app)
    {
        // Apply migrations
        app.UseMigration<YourDbContext>();
        
        return app;
    }
}
```

### 2. Connection String Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=eshopdb;Username=user;Password=pass",
    "CatalogConnection": "Host=localhost;Database=eshopdb;Username=user;Password=pass;SearchPath=catalog",
    "BasketConnection": "Host=localhost;Database=eshopdb;Username=user;Password=pass;SearchPath=basket"
  }
}
```

### 3. Error Handling

```csharp
try 
{
    await app.UseMigrationAsync<YourDbContext>();
}
catch (Exception ex)
{
    logger.LogError(ex, "Migration failed for YourDbContext");
    // Handle gracefully - don't crash the application
}
```

### 4. Health Checks Integration

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CatalogDbContext>("catalog-db")
    .AddDbContextCheck<BasketDbContext>("basket-db");
```

## ?? Troubleshooting

### Common Issues

1. **Database Connection Failed**
   ```
   Error: Cannot connect to database for CatalogDbContext
   Solution: Check connection string and database availability
   ```

2. **Migration Timeout**
   ```
   Error: Migration attempt failed, retrying...
   Solution: Increase retryDelay or check for long-running migrations
   ```

3. **Schema Permissions**
   ```
   Error: Permission denied for schema 'catalog'
   Solution: Ensure database user has proper schema permissions
   ```

### Debug Migration Status

```bash
# Check migration status endpoint
curl http://localhost:8080/migration-status

# Check migration info
curl http://localhost:8080/migration-info
```

## ?? API Reference

### MigrationExtensions

- `UseMigration<T>()` - Apply migrations for specific DbContext
- `UseMigrationAsync<T>()` - Async version with configurable retry
- `UseMigrationsAsync()` - Apply migrations for multiple DbContexts
- `GetMigrationInfoAsync<T>()` - Get migration information

### ModuleMigrationExtensions

- `UseModuleMigrations()` - Auto-discover and migrate all DbContexts
- `UseModuleMigrationsAsync()` - Async version with retry configuration
- `GetAllMigrationInfoAsync()` - Get info for all registered DbContexts
- `MapMigrationStatusEndpoint()` - Add migration status endpoint

This migration system provides a robust, scalable foundation for database management in your modular monolith architecture! ??
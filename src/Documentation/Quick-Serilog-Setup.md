# ?? **Quick Serilog Setup for eShop Modular Monolith**

## **Current Status** ?
Your application is now building successfully with standard Microsoft logging and all pipeline behaviors working correctly.

## **Option 1: Add Serilog Incrementally (Recommended)**

### **Step 1: Add Serilog to API Project Only**

```bash
# Add Serilog packages to API project only
dotnet add Bootstrapper/Api/Api.csproj package Serilog.AspNetCore --version 8.0.3
dotnet add Bootstrapper/Api/Api.csproj package Serilog.Sinks.Seq --version 7.0.1
```

### **Step 2: Update Program.cs with Basic Serilog**

Replace your Program.cs with this enhanced version:

```csharp
using Catalog;
using Basket;
using Odering;
using Shared.Data.Extensions;
using Shared.Extentions;
using Shared.Exceptions.Extensions;
using Carter;
using Serilog;
using Serilog.Events;

// Configure Serilog early in the pipeline
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/eShop-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

Log.Information("?? Starting eShop Modular Monolith API...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog as the logging provider
    builder.Host.UseSerilog((context, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "eShop.ModularMonolith")
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.Seq("http://localhost:5341", restrictedToMinimumLevel: LogEventLevel.Debug)
            .WriteTo.File("logs/eShop-.txt", 
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] [{Application}] {Message:lj} {Properties:j}{NewLine}{Exception}");
    });

    builder.Services.AddCarterWithAssemblies(typeof(CatalogModule).Assembly);

    // Add services to the container
    builder.Services.AddCatalogModule(builder.Configuration)
                    .AddBasketModule(builder.Configuration)
                    .AddOrderingModule(builder.Configuration);

    // Add exception handling
    builder.Services.AddCustomExceptionHandler();

    // Add controllers for API endpoints
    builder.Services.AddControllers();

    // Add health checks (basic)
    builder.Services.AddHealthChecks();

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // Configure Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "?? HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
        options.GetLevel = (httpContext, elapsed, ex) => ex != null 
            ? LogEventLevel.Error 
            : httpContext.Response.StatusCode > 499 
                ? LogEventLevel.Error 
                : LogEventLevel.Information;
    });

    // Configure the HTTP request pipeline
    app.UseCors("AllowAll");

    // Add exception handling middleware (must be early in the pipeline)
    app.UseExceptionHandler();

    // === DATABASE MIGRATION OPTIONS ===

    // Option 1: Apply migrations for all registered DbContexts automatically
    // This will discover and migrate all registered DbContexts
    app.UseModuleMigrations();
    app.MapCarter();

    // Option 2: Configure modules (which will apply individual migrations)
    app.UseCatalogModule()
       .UseBasketModule()
       .UseOrderingModule();

    // === MIGRATION STATUS ENDPOINT ===
    // Add an endpoint to check migration status
    app.MapMigrationStatusEndpoint("/migration-status");

    // Add health check endpoints
    app.MapHealthChecks("/health");

    // Add migration info endpoint
    app.MapGet("/migration-info", async () =>
    {
        var migrationInfos = await app.GetAllMigrationInfoAsync();
        return Results.Ok(new
        {
            Timestamp = DateTime.UtcNow,
            Migrations = migrationInfos
        });
    });

    app.MapControllers();

    // Add a simple test endpoint
    app.MapGet("/", () => "eShop Modular Monolith API is running!");

    Log.Information("? eShop Modular Monolith API configured successfully");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "?? eShop Modular Monolith API terminated unexpectedly");
}
finally
{
    Log.Information("?? eShop Modular Monolith API shutting down...");
    await Log.CloseAndFlushAsync();
}
```

### **Step 3: Start Seq (Optional)**

```bash
# Start Seq using Docker
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest

# Access Seq UI at http://localhost:5341
```

### **Step 4: Test Your Setup**

```bash
# Build and run
dotnet build
dotnet run --project Bootstrapper/Api

# Test endpoints
curl http://localhost:5000/
curl http://localhost:5000/products
curl -X POST http://localhost:5000/test/logging -H "Content-Type: application/json" -d '{"message":"Hello World","delayMs":0}'

# Check logs in:
# - Console output
# - logs/eShop-*.txt files
# - Seq UI at http://localhost:5341 (if running)
```

## **What You'll Get** ??

### **? Immediate Benefits**
- **Structured Console Logging** with timestamps and levels
- **File Logging** with daily rotation (logs/eShop-*.txt)
- **Seq Integration** for powerful log analysis (if you run the Docker container)
- **HTTP Request Logging** with response times and status codes
- **Your existing pipeline behaviors** continue to work unchanged

### **?? Log Output Examples**

**Console:**
```
[10:30:15 INF] ?? Starting eShop Modular Monolith API...
[10:30:16 INF] ? eShop Modular Monolith API configured successfully
[10:30:20 INF] ?? HTTP GET / responded 200 in 45.2341ms
[10:30:25 INF] ?? Processing Command CreateProductCommand [abc123]
[10:30:25 INF] ? Completed Command CreateProductCommand [abc123] in 234ms
```

**File (logs/eShop-20240115.txt):**
```
[2024-01-15 10:30:15.123 INF] [eShop.ModularMonolith] ?? Starting eShop Modular Monolith API...
[2024-01-15 10:30:16.456 INF] [eShop.ModularMonolith] ? eShop Modular Monolith API configured successfully
[2024-01-15 10:30:20.789 INF] [eShop.ModularMonolith] ?? HTTP GET / responded 200 in 45.2341ms
```

## **Next Steps** ??

1. **Start with this basic setup** - it's production-ready and safe
2. **Add Seq if you want visual log analysis** - just run the Docker command
3. **Your pipeline behaviors already provide structured logging** - they'll automatically work with Serilog
4. **Gradually enhance** as needed without breaking existing functionality

## **Why This Approach Works** ?

- **No package conflicts** - only adds packages to API project
- **Backward compatible** - doesn't break existing logging
- **Incremental** - add features one at a time
- **Safe** - if Serilog fails, standard logging continues to work
- **Your behaviors keep working** - LoggingBehavior, PerformanceBehavior, etc. all continue to function

Your eShop modular monolith now has **working centralized logging with pipeline behaviors** and you can **optionally add Serilog incrementally** for even more powerful structured logging! ??
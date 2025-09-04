# ?? Serilog & Seq Implementation Guide for eShop Modular Monolith

## ?? **Overview**

This guide provides complete instructions for implementing Serilog structured logging with Seq for your eShop modular monolith, replacing the default Microsoft.Extensions.Logging with a more powerful structured logging solution.

## ?? **Step 1: Package Installation**

### **1.1 Update Shared Project (Modules/Shared/Shared/Shared.csproj)**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
	<ItemGroup>
		<FrameworkReference Include="Microsoft.AspNetCore.App" />
		<PackageReference Include="Carter" Version="8.0.0" />
		<PackageReference Include="FluentValidation" Version="11.9.0" />
		<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
		<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.0" />
		<PackageReference Include="Mapster" Version="7.4.0" />
		<PackageReference Include="MediatR" Version="13.0.0" />
		<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
		<PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version="8.0.11" />
		<PackageReference Include="Microsoft.Extensions.Hosting.Abstractions" Version="8.0.1" />
		<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.2" />
		
		<!-- Serilog Packages -->
		<PackageReference Include="Serilog" Version="4.3.0" />
		<PackageReference Include="Serilog.Extensions.Logging" Version="8.0.0" />
		<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
		<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
		<PackageReference Include="Serilog.Sinks.Seq" Version="9.0.0" />
		<PackageReference Include="Serilog.Enrichers.Environment" Version="3.0.1" />
		<PackageReference Include="Serilog.Enrichers.Process" Version="3.0.0" />
		<PackageReference Include="Serilog.Enrichers.Thread" Version="4.0.0" />
		<PackageReference Include="Serilog.Settings.Configuration" Version="8.0.4" />
	</ItemGroup>

</Project>
```

### **1.2 Update API Project (Bootstrapper/Api/Api.csproj)**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UserSecretsId>edb277a6-4729-45d9-b155-428bebc7f232</UserSecretsId>
    <DockerDefaultTargetOS>Linux</DockerDefaultTargetOS>
    <DockerfileContext>..\..</DockerfileContext>
    <DockerComposeProjectPath>..\..\docker-compose.dcproj</DockerComposeProjectPath>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.VisualStudio.Azure.Containers.Tools.Targets" Version="1.22.1" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="AspNetCore.HealthChecks.NpgSql" Version="8.0.1" />
    <PackageReference Include="AspNetCore.HealthChecks.Redis" Version="8.0.1" />
    <PackageReference Include="Microsoft.AspNetCore.Diagnostics.HealthChecks" Version="2.2.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.11" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Modules\Basket\Basket\Basket.csproj" />
    <ProjectReference Include="..\..\Modules\Catalog\Catalog\Catalog.csproj" />
    <ProjectReference Include="..\..\Modules\Oredering\Odering\Odering.csproj" />
    <ProjectReference Include="..\..\Modules\Shared\Shared\Shared.csproj" />
  </ItemGroup>

</Project>
```

## ?? **Step 2: Configuration Files**

### **2.1 appsettings.json**

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File", "Serilog.Sinks.Seq"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning",
        "Shared.Behaviors": "Information",
        "Catalog.Products": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console",
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341",
          "restrictedToMinimumLevel": "Debug"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/eShop-.txt",
          "rollingInterval": "Day",
          "rollOnFileSizeLimit": true,
          "fileSizeLimitBytes": 52428800,
          "retainedFileCountLimit": 7,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{Application}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithEnvironmentName",
      "WithMachineName", 
      "WithProcessId",
      "WithProcessName",
      "WithThreadId"
    ],
    "Properties": {
      "Application": "eShop.ModularMonolith"
    }
  },
  
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=EShop;Username=postgres;Password=postgres;"
  },
  
  "AllowedHosts": "*"
}
```

### **2.2 appsettings.Development.json**

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "Microsoft.EntityFrameworkCore": "Information",
        "Shared.Behaviors": "Debug",
        "Catalog.Products": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console",
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341",
          "restrictedToMinimumLevel": "Verbose"
        }
      }
    ],
    "Properties": {
      "Application": "eShop.ModularMonolith.Development"
    }
  },
  
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=EShopDev;Username=postgres;Password=postgres;"
  }
}
```

## ??? **Step 3: Program.cs Configuration**

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
    .WriteTo.Console()
    .WriteTo.File("logs/startup-.txt", rollingInterval: RollingInterval.Day)
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
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId();
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
        
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault());
            diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
            
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserId", httpContext.User.Identity.Name);
            }
        };
    });

    // Configure the HTTP request pipeline
    app.UseCors("AllowAll");
    app.UseExceptionHandler();

    // Database migrations and Carter
    app.UseModuleMigrations();
    app.MapCarter();

    // Configure modules
    app.UseCatalogModule()
       .UseBasketModule()
       .UseOrderingModule();

    // Health checks and endpoints
    app.MapMigrationStatusEndpoint("/migration-status");
    app.MapHealthChecks("/health");
    
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

## ?? **Step 4: Docker Setup for Seq**

### **4.1 docker-compose.logging.yml**

```yaml
version: '3.8'

services:
  seq:
    image: datalust/seq:2024.1
    container_name: eShop-seq
    environment:
      - ACCEPT_EULA=Y
      - SEQ_FIRSTRUN_ADMINPASSWORDHASH=# Leave empty for no password initially
    ports:
      - "5341:80"
    volumes:
      - seq-data:/data
    restart: unless-stopped
    networks:
      - eshop-network

  postgres:
    image: postgres:15
    container_name: eShop-postgres
    environment:
      POSTGRES_DB: EShop
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
    restart: unless-stopped
    networks:
      - eshop-network

volumes:
  seq-data:
  postgres-data:

networks:
  eshop-network:
    driver: bridge
```

### **4.2 Start Services**

```bash
# Start Seq and PostgreSQL
docker-compose -f docker-compose.logging.yml up -d

# Check status
docker ps

# View Seq logs
docker logs eShop-seq

# Access Seq UI at http://localhost:5341
```

## ?? **Step 5: Enhanced Logging Behavior**

Create a custom Serilog behavior in `Modules/Shared/Shared/Behaviors/SerilogBehavior.cs`:

```csharp
using System.Diagnostics;
using System.Text.Json;
using Serilog.Context;
using Microsoft.Extensions.Logging;

namespace Shared.Behaviors
{
    public class SerilogBehavior<TRequest, TResponse>(ILogger<SerilogBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var requestId = Guid.NewGuid().ToString("N")[..12];
            var stopwatch = Stopwatch.StartNew();

            // Use Serilog's LogContext for structured properties
            using (LogContext.PushProperty("RequestId", requestId))
            using (LogContext.PushProperty("RequestName", requestName))
            using (LogContext.PushProperty("RequestType", GetRequestType(requestName)))
            {
                logger.LogInformation("?? Processing {RequestType} {RequestName} [{RequestId}]",
                    GetRequestType(requestName), requestName, requestId);

                try
                {
                    var response = await next();
                    
                    stopwatch.Stop();

                    logger.LogInformation("? Completed {RequestType} {RequestName} [{RequestId}] in {ElapsedMs}ms",
                        GetRequestType(requestName), requestName, requestId, stopwatch.ElapsedMilliseconds);

                    LogPerformanceMetrics(requestName, requestId, stopwatch.ElapsedMilliseconds);

                    return response;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();

                    logger.LogError(ex, "? Failed {RequestType} {RequestName} [{RequestId}] in {ElapsedMs}ms",
                        GetRequestType(requestName), requestName, requestId, stopwatch.ElapsedMilliseconds);

                    throw;
                }
            }
        }

        private void LogPerformanceMetrics(string requestName, string requestId, long elapsedMs)
        {
            using (LogContext.PushProperty("PerformanceCategory", GetPerformanceCategory(elapsedMs)))
            {
                if (elapsedMs > 5000)
                {
                    logger.LogWarning("?? Slow request: {RequestName} [{RequestId}] took {ElapsedMs}ms",
                        requestName, requestId, elapsedMs);
                }
                else if (elapsedMs > 1000)
                {
                    logger.LogInformation("?? Request {RequestName} [{RequestId}] took {ElapsedMs}ms",
                        requestName, requestId, elapsedMs);
                }
            }
        }

        private static string GetRequestType(string requestName) =>
            requestName.EndsWith("Command") ? "Command" :
            requestName.EndsWith("Query") ? "Query" : "Request";

        private static string GetPerformanceCategory(long elapsedMs) =>
            elapsedMs > 5000 ? "Slow" :
            elapsedMs > 1000 ? "Moderate" :
            elapsedMs > 500 ? "Normal" : "Fast";
    }
}
```

## ?? **Step 6: Seq Queries & Dashboards**

Once your application is running and sending logs to Seq, you can use these queries:

### **6.1 Common Queries**

```sql
-- All requests in the last hour
@Timestamp > Now() - 1h

-- Failed requests
@Level = 'Error'

-- Slow requests
PerformanceCategory = 'Slow'

-- Requests by type
select @Timestamp, RequestName, RequestType, ElapsedMs
from stream
where RequestType is not null

-- Performance analysis
select RequestName, avg(ElapsedMs), count(*)
from stream
where ElapsedMs is not null
group by RequestName
order by avg(ElapsedMs) desc

-- Error analysis
select @Exception.Type, count(*)
from stream
where @Level = 'Error'
group by @Exception.Type
```

### **6.2 Dashboard Widgets**

Create dashboards in Seq with these widgets:

1. **Request Volume**: Line chart showing requests per minute
2. **Error Rate**: Percentage of failed requests
3. **Performance**: Average response times by endpoint
4. **Top Errors**: Most common exceptions
5. **User Activity**: Requests by authenticated users

## ?? **Benefits Achieved**

### **?? Structured Logging**
- **Rich Context**: Every log entry has structured properties
- **Searchable**: Query logs like a database
- **Correlatable**: Track requests across distributed systems

### **?? Performance Monitoring**
- **Real-time Metrics**: See performance issues as they happen
- **Historical Analysis**: Trend analysis over time
- **Alerting**: Set up alerts for slow requests or errors

### **?? Developer Experience**
- **Visual Interface**: Seq provides a rich UI for log exploration
- **Powerful Queries**: SQL-like queries for log analysis
- **Export/Import**: Export queries and dashboards

### **?? Production Ready**
- **High Performance**: Structured logging with minimal overhead
- **Scalable**: Seq can handle high-volume logging
- **Reliable**: Persistent storage with retention policies

## ?? **Commands to Get Started**

```bash
# 1. Start Seq (if using Docker)
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest

# 2. Build and run your application
dotnet build
dotnet run --project Bootstrapper/Api

# 3. Access Seq UI
# Open http://localhost:5341 in your browser

# 4. Test your endpoints
curl http://localhost:5000/products
curl http://localhost:5000/test/logging -X POST -H "Content-Type: application/json" -d '{"message":"Hello Serilog","delayMs":0}'

# 5. View logs in Seq
# Go to http://localhost:5341 and explore your structured logs!
```

Your eShop modular monolith is now ready for **enterprise-grade structured logging** with Serilog and Seq! ??
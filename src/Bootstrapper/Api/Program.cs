using Catalog;
using Basket;
using Odering;
using Shared.Data.Extensions;
using Shared.Extentions;
using Shared.Exceptions.Extensions;
using Carter;
using Shared.Behaviors;

var builder = WebApplication.CreateBuilder(args);

var catlogAssembly = typeof(CatalogModule).Assembly;
var basketsAssembly = typeof(CatalogModule).Assembly;

//common sevices: carter, Mediater, FluentValidation
builder.Services.AddCarterWithAssemblies(catlogAssembly, basketsAssembly);

builder.Services.AddMediatRWithAssemblies(catlogAssembly, basketsAssembly);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});


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

// Option 3: Apply migrations manually with custom retry settings
// await app.UseMigrationAsync<CatalogDbContext>(retryCount: 5, retryDelay: 3000);
// await app.UseMigrationAsync<BasketDbContext>(retryCount: 5, retryDelay: 3000);

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

app.Run();

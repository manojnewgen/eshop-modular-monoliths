using Catalog;
using Basket;
using Odering; // Note: This appears to be the actual namespace name in the project
using Shared.Data.Extensions;
using Shared.Behaviors.Extensions; // CHANGED: Use the correct extension namespace
using Shared.Exceptions.Extensions;
using Carter;
using Shared.Behaviors;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using FluentValidation;
using Keycloak.AuthServices.Authentication; // ADDED: For validator registration

var builder = WebApplication.CreateBuilder(args);

// Assembly references - Fixed typos and improved naming
var catalogAssembly = typeof(CatalogModule).Assembly;  // Fixed typo: was "catlogAssembly"
var basketAssembly = typeof(BasketModule).Assembly;    // Fixed: was typeof(CatalogModule).Assembly
var orderingAssembly = typeof(OrderingModule).Assembly; // Added missing ordering assembly

var moduleAssemblies = new[] { catalogAssembly, basketAssembly, orderingAssembly };

// ADDED: Authentication and Authorization Configuration
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var keycloakConfig = builder.Configuration.GetSection("Authentication:Keycloak");
        
        options.Authority = keycloakConfig["Authority"]; // http://localhost:9090/realms/eshop
        options.Audience = keycloakConfig["Audience"];   // eshop-api
        options.RequireHttpsMetadata = keycloakConfig.GetValue<bool>("RequireHttpsMetadata", false);
        
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = keycloakConfig.GetValue<bool>("ValidateIssuer", true),
            ValidateAudience = keycloakConfig.GetValue<bool>("ValidateAudience", false),
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5) // Allow 5 minutes clock skew
        };

        // Development-specific settings
        if (builder.Environment.IsDevelopment())
        {
            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    builder.Services.BuildServiceProvider()
                        .GetRequiredService<ILogger<Program>>()
                        .LogError("Authentication failed: {Error}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    builder.Services.BuildServiceProvider()
                        .GetRequiredService<ILogger<Program>>()
                        .LogInformation("Token validated for user: {User}", 
                            context.Principal?.Identity?.Name ?? "Unknown");
                    return Task.CompletedTask;
                }
            };
        }
    });

 builder.Services.AddKeycloakWebApiAuthentication(builder.Configuration);

//builder.Services.AddAuthorization(options =>
//{
//    // Add default policy requiring authentication for all endpoints except explicitly allowed
//    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
//        .RequireAuthenticatedUser()
//        .Build();
        
//    // Add custom policies as needed
//    options.AddPolicy("AdminOnly", policy => 
//        policy.RequireClaim("realm_access", "admin"));
        
//    options.AddPolicy("UserAccess", policy => 
//        policy.RequireClaim("realm_access", "user"));
//});

// Common services: Carter, MediatR, FluentValidation
builder.Services.AddCarterWithAssemblies(moduleAssemblies);
// CHANGED: Use the correct extension method that properly registers pipeline behaviors
builder.Services.AddMediatRWithBehaviors(moduleAssemblies);

// ADDED: Explicit FluentValidation registration
builder.Services.AddValidatorsFromAssemblies(moduleAssemblies);

// Redis Cache with better configuration
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "eShop";
});

// Message Bus
builder.Services.AddMassTransitWithAssemblies(builder.Configuration, moduleAssemblies);

// Module services
builder.Services.AddCatalogModule(builder.Configuration)
                .AddBasketModule(builder.Configuration)
                .AddOrderingModule(builder.Configuration);

// Exception handling
builder.Services.AddCustomExceptionHandler();

// API Controllers with improved JSON configuration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// API Documentation for non-production environments
if (!builder.Environment.IsProduction())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo 
        { 
            Title = "eShop Modular Monolith API", 
            Version = "v1",
            Description = "A modular monolith e-commerce API built with .NET 8",
            Contact = new OpenApiContact
            {
                Name = "eShop Team",
                Email = "support@eshop.com"
            }
        });
        
        // ADDED: JWT Bearer configuration for Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer {token}'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
        
        // Include XML comments if available
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    });
}

// Enhanced Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

// Add database health checks if connection strings are available
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
var redisConnection = builder.Configuration.GetConnectionString("Redis");

if (!string.IsNullOrEmpty(defaultConnection))
{
    builder.Services.AddHealthChecks()
        .AddNpgSql(defaultConnection, name: "database", tags: new[] { "database", "ready" });
}

if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddHealthChecks()
        .AddRedis(redisConnection, name: "redis", tags: new[] { "cache", "ready" });
}

// Environment-specific CORS configuration
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("Development", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    }
    else
    {
        // Production CORS - more restrictive
        options.AddPolicy("Production", policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
                               ?? new[] { "https://localhost:7000", "https://localhost:7001" };
            
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()
                  .SetPreflightMaxAge(TimeSpan.FromMinutes(5));
        });
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline based on environment
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "eShop API V1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
        c.OAuthClientId("eshop-api"); // For Keycloak integration
        c.OAuthRealm("eshop");
    });
    app.UseCors("Development");
}
else
{
    // Production security headers
    app.UseHsts();
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        await next();
    });
    app.UseCors("Production");
}

app.UseHttpsRedirection();

// Exception handling middleware (must be early in the pipeline)
app.UseExceptionHandler();

// ADDED: Authentication and Authorization middleware
app.UseAuthentication(); // Must come before UseAuthorization
app.UseAuthorization();

// Database migrations with better error handling
try
{
    app.Logger.LogInformation("Starting database migrations...");
    app.UseModuleMigrations();
    app.Logger.LogInformation("Database migrations completed successfully");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Database migration failed during startup");
    if (app.Environment.IsProduction())
    {
        throw; // Re-throw in production to prevent startup with bad database state
    }
    app.Logger.LogWarning("Continuing startup despite migration failure (Development environment)");
}

// Configure modules in dependency order
app.UseCatalogModule()
   .UseBasketModule()
   .UseOrderingModule();

// Carter routing for minimal APIs
app.MapCarter();

// Comprehensive Health Check endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
{
    Predicate = _ => true,
    AllowCachingResponses = false
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
{
    Predicate = check => check.Tags.Contains("ready"),
    AllowCachingResponses = false
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
{
    Predicate = _ => false, // No checks for liveness - just that the app is running
    AllowCachingResponses = false
});

// Migration status endpoint
app.MapMigrationStatusEndpoint("/migration-status");

// Enhanced migration info endpoint with better error handling
app.MapGet("/migration-info", async (ILogger<Program> logger) =>
{
    try
    {
        var migrationInfos = await app.GetAllMigrationInfoAsync();
        return Results.Ok(new
        {
            Timestamp = DateTime.UtcNow,
            Environment = app.Environment.EnvironmentName,
            Version = "1.0.0",
            Status = "Running",
            Migrations = migrationInfos
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to retrieve migration information");
        return Results.Problem(
            title: "Migration Info Error",
            detail: "Failed to retrieve migration information. Check logs for details.",
            statusCode: 500);
    }
})
.WithName("GetMigrationInfo")
.WithTags("System");

// API Controllers
app.MapControllers();

// Enhanced root endpoint with comprehensive API information
app.MapGet("/", (IWebHostEnvironment env, LinkGenerator linkGenerator, HttpContext context) => 
{
    var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
    
    return Results.Ok(new
    {
        Message = "?? eShop Modular Monolith API is running!",
        Version = "1.0.0",
        Environment = env.EnvironmentName,
        Timestamp = DateTime.UtcNow,
        Server = Environment.MachineName,
        Framework = Environment.Version.ToString(),
        Authentication = new
        {
            Enabled = true,
            Provider = "Keycloak",
            Authority = app.Configuration["Authentication:Keycloak:Authority"],
            TokenEndpoint = $"{app.Configuration["Authentication:Keycloak:Authority"]}/protocol/openid_connect/token"
        },
        Endpoints = new
        {
            Health = $"{baseUrl}/health",
            HealthReady = $"{baseUrl}/health/ready",
            HealthLive = $"{baseUrl}/health/live",
            MigrationStatus = $"{baseUrl}/migration-status",
            MigrationInfo = $"{baseUrl}/migration-info",
            Swagger = env.IsDevelopment() ? $"{baseUrl}/swagger" : null,
            Controllers = $"{baseUrl}/api"
        },
        Modules = new
        {
            Catalog = "Product catalog management",
            Basket = "Shopping cart functionality", 
            Ordering = "Order processing and management"
        }
    });
})
.WithName("GetApiInfo")
.WithTags("System")
.AllowAnonymous(); // Allow anonymous access to root endpoint

// Graceful shutdown handling
var appLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

appLifetime.ApplicationStopping.Register(() =>
{
    app.Logger.LogInformation("?? Application is shutting down...");
});

appLifetime.ApplicationStopped.Register(() =>
{
    app.Logger.LogInformation("? Application has stopped gracefully");
});

try
{
    app.Logger.LogInformation("?? Starting eShop Modular Monolith API");
    app.Logger.LogInformation("??? Environment: {Environment}", app.Environment.EnvironmentName);
    app.Logger.LogInformation("?? Content Root: {ContentRoot}", app.Environment.ContentRootPath);
    app.Logger.LogInformation("?? Authentication: Enabled (Keycloak)");
    
    app.Run();
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "?? Application terminated unexpectedly");
    throw;
}

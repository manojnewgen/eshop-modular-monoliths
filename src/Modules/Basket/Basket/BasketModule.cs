using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Basket.Data;
using Shared.Data.Extensions;
using Basket.Data.Repositories;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket
{
    public static class BasketModule
    {
        /// <summary>
        /// Registers Basket module services including DbContext
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddBasketModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IBasketRepository, BasketRepository>();

            //services.AddScoped<IBasketRepository>(provider=>
            //{
            //    var BaseBasketRepository = provider.GetRequiredService<BasketRepository>();
            //    return new CachedBasketRepository(BaseBasketRepository, provider.GetRequiredService<IDistributedCache>());
            //});

            services.Decorate<IBasketRepository, CachedBasketRepository>();

            // api end point service
            // aplication use cases services

            // Data -infra strcture related services 

            // Register DbContext with connection string
            services.AddDbContext<BasketDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("BasketConnection") 
                                     ?? configuration.GetConnectionString("DefaultConnection");
                
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "basket");
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                });

                // Enable sensitive data logging in development
                if (configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            // Register other basket services here
            // services.AddScoped<IBasketService, BasketService>();
            // services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();

            return services;
        }

        /// <summary>
        /// Configures Basket module middleware and applies database migrations
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The application builder for method chaining</returns>
        public static IApplicationBuilder UseBasketModule(this IApplicationBuilder app)
        {
            // Apply database migrations for Basket module
            app.UseMigration<BasketDbContext>();

            // Register other middleware here if needed
            // app.UseMiddleware<BasketMiddleware>();

            return app;
        }

        /// <summary>
        /// Applies database migrations for the Basket module asynchronously
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="retryCount">Number of retry attempts if migration fails (default: 3)</param>
        /// <param name="retryDelay">Delay between retry attempts in milliseconds (default: 5000)</param>
        /// <returns>The application builder for method chaining</returns>
        public static async Task<IApplicationBuilder> InitializeDatabaseAsync(
            this IApplicationBuilder app,
            int retryCount = 3,
            int retryDelay = 5000)
        {
            return await app.UseMigrationAsync<BasketDbContext>(retryCount, retryDelay);
        }

        /// <summary>
        /// Gets migration information for the Basket module
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>Migration information</returns>
        public static async Task<MigrationInfo> GetBasketMigrationInfoAsync(this IApplicationBuilder app)
        {
            return await app.GetMigrationInfoAsync<BasketDbContext>();
        }
    }
}

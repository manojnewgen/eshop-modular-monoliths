using Catalog.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Shared.Data.Extensions;
using Shared.Data;
using Shared.Behaviors.Extensions;

namespace Catalog
{
    /// <summary>
    /// Catalog module registration and configuration extensions
    /// Provides methods to register Catalog services and configure the module
    /// </summary>
    public static class CatalogModule
    {
        /// <summary>
        /// Registers Catalog module services including DbContext, MediatR, and Mapster
        /// </summary>
        /// <param name="serviceCollection">The service collection to register services into</param>
        /// <param name="configuration">The application configuration</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddCatalogModule(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            // Register Entity Framework DbContext with PostgreSQL
            serviceCollection.AddDbContext<CatalogDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("CatalogConnection") 
                                     ?? configuration.GetConnectionString("DefaultConnection");
                
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "catalog");
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                });
            });

            // Register MediatR with pipeline behaviors for command/query handling
            serviceCollection.AddMediatRWithBehaviors(Assembly.GetExecutingAssembly());

            // Register Mapster with Catalog assembly for mapping configurations
            serviceCollection.AddMapster(Assembly.GetExecutingAssembly());

            return serviceCollection;
        }

        /// <summary>
        /// Configures Catalog module middleware and applies database migrations
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The application builder for method chaining</returns>
        public static IApplicationBuilder UseCatalogModule(this IApplicationBuilder app)
        {
            // Apply database migrations for Catalog module using the new generic extension
            app.UseMigration<CatalogDbContext>();

            // Register other middleware here if needed
            // app.UseMiddleware<CatalogMiddleware>();

            return app;
        }

        /// <summary>
        /// Applies database migrations for the Catalog module asynchronously
        /// This is the improved version of the old InitialiizedatabaseAsync method
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
            return await app.UseMigrationAsync<CatalogDbContext>(retryCount, retryDelay);
        }

        /// <summary>
        /// Gets migration information for the Catalog module
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>Migration information</returns>
        public static async Task<MigrationInfo> GetCatalogMigrationInfoAsync(this IApplicationBuilder app)
        {
            return await app.GetMigrationInfoAsync<CatalogDbContext>();
        }

        /// <summary>
        /// ⚠️ DEPRECATED: Use InitializeDatabaseAsync instead
        /// This method name had a typo and has been replaced
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The application builder for method chaining</returns>
        [Obsolete("Use InitializeDatabaseAsync instead. This method name had a typo and will be removed in future versions.")]
        public static async Task InitialiizedatabaseAsync(this IApplicationBuilder app)
        {
            await app.InitializeDatabaseAsync();
        }
    }
}

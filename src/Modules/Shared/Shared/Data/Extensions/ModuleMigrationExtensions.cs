using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Shared.Data.Extensions
{
    /// <summary>
    /// Module-specific migration extensions that provide convenient methods for modular monolith applications
    /// </summary>
    public static class ModuleMigrationExtensions
    {
        /// <summary>
        /// Applies migrations for all registered module DbContexts
        /// This method discovers all DbContext types in the service collection and applies their migrations
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="retryCount">Number of retry attempts if migration fails (default: 3)</param>
        /// <param name="retryDelay">Delay between retry attempts in milliseconds (default: 5000)</param>
        /// <returns>The application builder for method chaining</returns>
        public static async Task<IApplicationBuilder> UseModuleMigrationsAsync(
            this IApplicationBuilder app,
            int retryCount = 3,
            int retryDelay = 5000)
        {
            var dbContextTypes = GetRegisteredDbContextTypes(app);
            
            if (dbContextTypes.Any())
            {
                await app.UseMigrationsAsync(dbContextTypes.ToArray(), retryCount, retryDelay);
            }

            return app;
        }

        /// <summary>
        /// Synchronous version that schedules migrations to run in background
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="retryCount">Number of retry attempts if migration fails (default: 3)</param>
        /// <param name="retryDelay">Delay between retry attempts in milliseconds (default: 5000)</param>
        /// <returns>The application builder for method chaining</returns>
        public static IApplicationBuilder UseModuleMigrations(
            this IApplicationBuilder app,
            int retryCount = 3,
            int retryDelay = 5000)
        {
            _ = Task.Run(async () => await app.UseModuleMigrationsAsync(retryCount, retryDelay));
            return app;
        }

        /// <summary>
        /// Gets migration information for all registered DbContexts
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>List of migration information for all DbContexts</returns>
        public static async Task<List<MigrationInfo>> GetAllMigrationInfoAsync(this IApplicationBuilder app)
        {
            var dbContextTypes = GetRegisteredDbContextTypes(app);
            var migrationInfos = new List<MigrationInfo>();

            foreach (var contextType in dbContextTypes)
            {
                try
                {
                    // Use reflection to call the generic method
                    var method = typeof(MigrationExtensions)
                        .GetMethod(nameof(MigrationExtensions.GetMigrationInfoAsync))!
                        .MakeGenericMethod(contextType);

                    var task = (Task<MigrationInfo>)method.Invoke(null, new object[] { app })!;
                    var migrationInfo = await task;
                    migrationInfos.Add(migrationInfo);
                }
                catch (Exception ex)
                {
                    migrationInfos.Add(new MigrationInfo
                    {
                        ContextName = contextType.Name,
                        CanConnect = false,
                        Error = ex.Message
                    });
                }
            }

            return migrationInfos;
        }

        /// <summary>
        /// Creates a migration status endpoint that can be used for health checks
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="endpoint">The endpoint path (default: "/migration-status")</param>
        /// <returns>The application builder for method chaining</returns>
        public static IApplicationBuilder MapMigrationStatusEndpoint(
            this IApplicationBuilder app,
            string endpoint = "/migration-status")
        {
            return app.Map(endpoint, builder =>
            {
                builder.Run(async context =>
                {
                    var migrationInfos = await app.GetAllMigrationInfoAsync();
                    
                    var result = new
                    {
                        Timestamp = DateTime.UtcNow,
                        Status = migrationInfos.All(m => m.CanConnect && !m.HasPendingMigrations) ? "Healthy" : "Unhealthy",
                        Contexts = migrationInfos.Select(m => new
                        {
                            m.ContextName,
                            m.CanConnect,
                            m.HasPendingMigrations,
                            AppliedMigrationsCount = m.AppliedMigrations.Count,
                            PendingMigrationsCount = m.PendingMigrations.Count,
                            m.Error,
                            PendingMigrations = m.HasPendingMigrations ? m.PendingMigrations : null
                        })
                    };

                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }));
                });
            });
        }

        private static List<Type> GetRegisteredDbContextTypes(IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices;
            var dbContextTypes = new List<Type>();

            // Scan assemblies for DbContext types that might be registered
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var contextTypes = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(DbContext)))
                        .ToList();

                    foreach (var contextType in contextTypes)
                    {
                        // Check if this context is actually registered in DI
                        try
                        {
                            var service = serviceProvider.GetService(contextType);
                            if (service != null)
                            {
                                dbContextTypes.Add(contextType);
                            }
                        }
                        catch
                        {
                            // Context not registered, skip it
                        }
                    }
                }
                catch
                {
                    // Skip assemblies that can't be loaded
                }
            }

            return dbContextTypes.Distinct().ToList();
        }
    }
}
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shared.Data.Extensions
{
    /// <summary>
    /// Generic database migration extensions for modular monolith applications
    /// </summary>
    public static class MigrationExtensions
    {
        /// <summary>
        /// Applies pending migrations for the specified DbContext type
        /// </summary>
        /// <typeparam name="TContext">The DbContext type to migrate</typeparam>
        /// <param name="app">The application builder</param>
        /// <param name="retryCount">Number of retry attempts if migration fails (default: 3)</param>
        /// <param name="retryDelay">Delay between retry attempts in milliseconds (default: 5000)</param>
        /// <returns>The application builder for method chaining</returns>
        public static async Task<IApplicationBuilder> UseMigrationAsync<TContext>(
            this IApplicationBuilder app,
            int retryCount = 3,
            int retryDelay = 5000) where TContext : DbContext
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<TContext>>();
            var environment = services.GetRequiredService<IHostEnvironment>();

            try
            {
                var context = services.GetRequiredService<TContext>();
                var contextName = typeof(TContext).Name;

                logger.LogInformation("Starting database migration for {ContextName}", contextName);

                // Check if database can be connected to
                if (!await CanConnectToDatabase(context, logger, contextName))
                {
                    logger.LogWarning("Cannot connect to database for {ContextName}, skipping migration", contextName);
                    return app;
                }

                // Ensure schemas exist before applying migrations
                await EnsureSchemasExist(context, logger, contextName);

                // Apply migrations with retry logic
                await ApplyMigrationsWithRetry(context, logger, contextName, retryCount, retryDelay);

                logger.LogInformation("Database migration completed successfully for {ContextName}", contextName);
                seedData(context, services);
            }
            catch (Exception ex)
            {
                var contextName = typeof(TContext).Name;
                logger.LogError(ex, "An error occurred while migrating the database for {ContextName}", contextName);
                
                // In development, we might want to throw the exception to see what's wrong
                if (environment.IsDevelopment())
                {
                    logger.LogError("Migration failed in development environment. Exception details: {ExceptionMessage}", ex.Message);
                }
                
                // In production, we don't want to crash the application due to migration issues
                // The application should start and handle database issues gracefully
                if (!environment.IsDevelopment())
                {
                    logger.LogWarning("Migration failed in production environment. Application will continue startup.");
                }
            }

            return app;
        }

        /// <summary>
        /// Ensures all required schemas exist in the database
        /// </summary>
        private static async Task EnsureSchemasExist<TContext>(TContext context, ILogger logger, string contextName) 
            where TContext : DbContext
        {
            try
            {
                logger.LogInformation("Ensuring schemas exist for {ContextName}", contextName);

                var schemas = new[] { "catalog", "basket", "ordering", "identity", "shared", "messaging" };

                foreach (var schema in schemas)
                {
                    // Fix for CS1039 and CS1002: Close the string literal and add the missing semicolon
                    var schemaExistsQuery = $"SELECT EXISTS(SELECT 1 FROM information_schema.schemata WHERE schema_name = '{schema}');";
                    var connection = context.Database.GetDbConnection();
                    
                    if (connection.State != System.Data.ConnectionState.Open)
                        await connection.OpenAsync();

                    using var command = connection.CreateCommand();
                    command.CommandText = schemaExistsQuery;
                    
                    var exists = (bool)(await command.ExecuteScalarAsync() ?? false);
                    
                    if (!exists)
                    {
                        logger.LogInformation("Creating schema: {Schema}", schema);
                        command.CommandText = $"CREATE SCHEMA IF NOT EXISTS {schema}";
                        await command.ExecuteNonQueryAsync();
                    }
                }

                // Create extensions if needed
                await EnsureExtensionsExist(context, logger);
                
                logger.LogInformation("Schema creation completed for {ContextName}", contextName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error ensuring schemas exist for {ContextName}", contextName);
                throw;
            }
        }

        /// <summary>
        /// Ensures required PostgreSQL extensions exist
        /// </summary>
        private static async Task EnsureExtensionsExist<TContext>(TContext context, ILogger logger) 
            where TContext : DbContext
        {
            try
            {
                var extensions = new[] { "uuid-ossp", "pg_trgm" };
                var connection = context.Database.GetDbConnection();

                foreach (var extension in extensions)
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = $"CREATE EXTENSION IF NOT EXISTS \"{extension}\"";
                    await command.ExecuteNonQueryAsync();
                    logger.LogDebug("Ensured extension exists: {Extension}", extension);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not create extensions (this may be normal if user lacks permissions)");
            }
        }

        private static void seedData<TContext>(TContext context, IServiceProvider services) where TContext : DbContext
        {
            // Implementation for seeding initial data can be added here
            // This is called after successful migration
        }

        /// <summary>
        /// Applies pending migrations for multiple DbContext types
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="contextTypes">Array of DbContext types to migrate</param>
        /// <param name="retryCount">Number of retry attempts if migration fails (default: 3)</param>
        /// <param name="retryDelay">Delay between retry attempts in milliseconds (default: 5000)</param>
        /// <returns>The application builder for method chaining</returns>
        public static async Task<IApplicationBuilder> UseMigrationsAsync(
            this IApplicationBuilder app,
            Type[] contextTypes,
            int retryCount = 3,
            int retryDelay = 5000)
        {
            foreach (var contextType in contextTypes)
            {
                if (!contextType.IsSubclassOf(typeof(DbContext)))
                {
                    throw new ArgumentException($"Type {contextType.Name} is not a DbContext", nameof(contextTypes));
                }

                // Use reflection to call the generic method
                var method = typeof(MigrationExtensions)
                    .GetMethod(nameof(UseMigrationAsync))!
                    .MakeGenericMethod(contextType);

                await (Task<IApplicationBuilder>)method.Invoke(null, new object[] { app, retryCount, retryDelay })!;
            }

            return app;
        }

        /// <summary>
        /// Synchronous version of UseMigration - schedules the migration to run in background
        /// </summary>
        /// <typeparam name="TContext">The DbContext type to migrate</typeparam>
        /// <param name="app">The application builder</param>
        /// <param name="retryCount">Number of retry attempts if migration fails (default: 3)</param>
        /// <param name="retryDelay">Delay between retry attempts in milliseconds (default: 5000)</param>
        /// <returns>The application builder for method chaining</returns>
        public static IApplicationBuilder UseMigration<TContext>(
            this IApplicationBuilder app,
            int retryCount = 3,
            int retryDelay = 5000) where TContext : DbContext
        {
            // Schedule migration to run in background to avoid blocking startup
            _ = Task.Run(async () => await app.UseMigrationAsync<TContext>(retryCount, retryDelay));
            return app;
        }

        /// <summary>
        /// Gets information about pending migrations for the specified DbContext
        /// </summary>
        /// <typeparam name="TContext">The DbContext type</typeparam>
        /// <param name="app">The application builder</param>
        /// <returns>Information about pending migrations</returns>
        public static async Task<MigrationInfo> GetMigrationInfoAsync<TContext>(this IApplicationBuilder app)
            where TContext : DbContext
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<TContext>>();

            try
            {
                var context = services.GetRequiredService<TContext>();
                var contextName = typeof(TContext).Name;

                var appliedMigrations = context.Database.GetAppliedMigrations();
                var pendingMigrations = context.Database.GetPendingMigrations();
                var canConnect = context.Database.CanConnect();

                return new MigrationInfo
                {
                    ContextName = contextName,
                    CanConnect = canConnect,
                    AppliedMigrations = appliedMigrations.ToList(),
                    PendingMigrations = pendingMigrations.ToList(),
                    HasPendingMigrations = pendingMigrations.Any()
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting migration info for {ContextName}", typeof(TContext).Name);
                return new MigrationInfo
                {
                    ContextName = typeof(TContext).Name,
                    CanConnect = false,
                    Error = ex.Message
                };
            }
        }

        private static async Task<bool> CanConnectToDatabase<TContext>(
            TContext context,
            ILogger logger,
            string contextName) where TContext : DbContext
        {
            try
            {
                return await context.Database.CanConnectAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Cannot connect to database for {ContextName}: {ErrorMessage}", 
                    contextName, ex.Message);
                return false;
            }
        }

        private static async Task ApplyMigrationsWithRetry<TContext>(
            TContext context,
            ILogger logger,
            string contextName,
            int retryCount,
            int retryDelay) where TContext : DbContext
        {
            var attempts = 0;
            while (attempts <= retryCount)
            {
                try
                {
                    // Check if there are pending migrations
                    var pendingMigrations = context.Database.GetPendingMigrations();
                    var pendingMigrationsList = pendingMigrations.ToList();

                    if (!pendingMigrationsList.Any())
                    {
                        logger.LogInformation("No pending migrations for {ContextName}", contextName);
                        return;
                    }

                    logger.LogInformation("Applying {Count} pending migrations for {ContextName}: {Migrations}",
                        pendingMigrationsList.Count, contextName, string.Join(", ", pendingMigrationsList));

                    // Apply migrations
                    await context.Database.MigrateAsync();

                    logger.LogInformation("Successfully applied migrations for {ContextName}", contextName);
                    return;
                }
                catch (Exception ex) when (attempts < retryCount)
                {
                    attempts++;
                    logger.LogWarning(ex, 
                        "Migration attempt {Attempt} of {MaxAttempts} failed for {ContextName}. Retrying in {Delay}ms...",
                        attempts, retryCount + 1, contextName, retryDelay);

                    await Task.Delay(retryDelay);
                }
            }

            // If we get here, all retry attempts failed
            throw new InvalidOperationException(
                $"Failed to apply migrations for {contextName} after {retryCount + 1} attempts");
        }
    }

    /// <summary>
    /// Information about database migrations for a specific DbContext
    /// </summary>
    public class MigrationInfo
    {
        public string ContextName { get; set; } = string.Empty;
        public bool CanConnect { get; set; }
        public bool HasPendingMigrations { get; set; }
        public List<string> AppliedMigrations { get; set; } = new();
        public List<string> PendingMigrations { get; set; } = new();
        public string? Error { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Error))
                return $"{ContextName}: Error - {Error}";

            if (!CanConnect)
                return $"{ContextName}: Cannot connect to database";

            return $"{ContextName}: Applied={AppliedMigrations.Count}, Pending={PendingMigrations.Count}";
        }
    }
}
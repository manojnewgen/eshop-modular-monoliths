namespace Odering
{
    public static class OrderingModule
    {
        /// <summary>
        /// Registers Ordering module services
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddOrderingModule(this IServiceCollection services, IConfiguration configuration)
        {
            // TODO: Add DbContext registration when OrderingDbContext is created
            // services.AddDbContext<OrderingDbContext>(options =>
            // {
            //     var connectionString = configuration.GetConnectionString("OrderingConnection") 
            //                          ?? configuration.GetConnectionString("DefaultConnection");
            //     
            //     options.UseNpgsql(connectionString, npgsqlOptions =>
            //     {
            //         npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "ordering");
            //         npgsqlOptions.EnableRetryOnFailure(
            //             maxRetryCount: 3,
            //             maxRetryDelay: TimeSpan.FromSeconds(30),
            //             errorCodesToAdd: null);
            //     });
            // });

            // Register other ordering services here
            // services.AddScoped<IOrderingService, OrderingService>();

            return services;
        }

        /// <summary>
        /// Configures Ordering module middleware and applies database migrations
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The application builder for method chaining</returns>
        public static IApplicationBuilder UseOrderingModule(this IApplicationBuilder app)
        {
            // TODO: Apply database migrations when OrderingDbContext is created
            // app.UseMigration<OrderingDbContext>();

            // Register other middleware here if needed
            // app.UseMiddleware<OrderingMiddleware>();

            return app;
        }

        /// <summary>
        /// Applies database migrations for the Ordering module asynchronously
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
            // TODO: Uncomment when OrderingDbContext is created
            // return await app.UseMigrationAsync<OrderingDbContext>(retryCount, retryDelay);
            
            // For now, just return the app builder
            await Task.CompletedTask;
            return app;
        }
    }
}

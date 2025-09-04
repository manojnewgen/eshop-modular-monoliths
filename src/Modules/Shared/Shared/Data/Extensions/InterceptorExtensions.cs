using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Data.Interceptors;

namespace Shared.Data.Extensions
{
    public static class InterceptorExtensions
    {
        public static DbContextOptionsBuilder AddSaveChangesInterceptor(
            this DbContextOptionsBuilder optionsBuilder, 
            IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<SaveChangesInterceptor>>();
            var timeProvider = serviceProvider.GetService<TimeProvider>();
            
            var interceptor = new SaveChangesInterceptor(logger, serviceProvider, timeProvider);
            
            return optionsBuilder.AddInterceptors(interceptor);
        }

        public static DbContextOptionsBuilder AddSaveChangesInterceptor(
            this DbContextOptionsBuilder optionsBuilder,
            SaveChangesInterceptor interceptor)
        {
            return optionsBuilder.AddInterceptors(interceptor);
        }

        public static IServiceCollection AddSaveChangesInterceptor(this IServiceCollection services)
        {
            services.AddSingleton<TimeProvider>(TimeProvider.System);
            services.AddScoped<SaveChangesInterceptor>();
            
            return services;
        }
    }
}
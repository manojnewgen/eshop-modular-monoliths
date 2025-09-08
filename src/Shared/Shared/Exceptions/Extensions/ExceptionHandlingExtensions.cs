using Microsoft.Extensions.DependencyInjection;
using Shared.Exceptions.Handler;

namespace Shared.Exceptions.Extensions
{
    /// <summary>
    /// Extension methods for registering exception handling services
    /// </summary>
    public static class ExceptionHandlingExtensions
    {
        /// <summary>
        /// Registers the custom exception handler and related services
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddCustomExceptionHandler(this IServiceCollection services)
        {
            services.AddExceptionHandler<CustomExceptionHandler>();
            services.AddProblemDetails();
            
            return services;
        }
    }
}
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;

namespace Shared.Behaviors.Extensions
{
    /// <summary>
    /// Extension methods for registering MediatR pipeline behaviors
    /// </summary>
    public static class BehaviorExtensions
    {
        /// <summary>
        /// Registers all pipeline behaviors in the correct order:
        /// 1. Request Context (first - establishes correlation ID, user context)
        /// 2. Logging (second - logs everything with context)
        /// 3. Performance (third - tracks metrics)
        /// 4. Validation (fourth - validates before business logic)
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddPipelineBehaviors(this IServiceCollection services)
        {
            // Register HttpContextAccessor for RequestContextBehavior
            services.AddHttpContextAccessor();

            // Register behaviors in order of execution
            // Note: MediatR executes behaviors in REVERSE order of registration
            
            // Register Validation behavior (executes LAST, just before handler)
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            
            // Register Performance behavior (executes THIRD)
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            
            // Register Logging behavior (executes SECOND)
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            
            // Register Request Context behavior (executes FIRST - wraps everything)
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestContextBehavior<,>));

            return services;
        }

        /// <summary>
        /// Registers MediatR with all pipeline behaviors for the specified assemblies
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="assemblies">Assemblies to scan for handlers</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddMediatRWithBehaviors(this IServiceCollection services, params Assembly[] assemblies)
        {
            // Register MediatR
            services.AddMediatR(cfg =>
            {
                foreach (var assembly in assemblies)
                {
                    cfg.RegisterServicesFromAssembly(assembly);
                }
            });

            // Register all pipeline behaviors
            services.AddPipelineBehaviors();

            return services;
        }

        /// <summary>
        /// Registers only logging and performance behaviors (without validation)
        /// Use this when you want to register validation separately
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddLoggingAndPerformanceBehaviors(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestContextBehavior<,>));

            return services;
        }
    }
}
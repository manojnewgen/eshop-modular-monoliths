using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Extentions
{
    public static class MediatRExtension
    {
        public static IServiceCollection AddMediatRWithAssemblies(this IServiceCollection services, params Assembly[] assemblies)
        {
            // Register MediatR services
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(assemblies);
                cfg.AddBehavior(typeof(ValidationBehavior<,>));
                cfg.AddBehavior(typeof(LoggingBehavior<,>));

            });
            services. AddValidatorsFromAssemblies(assemblies);

            return services;
        }
    }
}

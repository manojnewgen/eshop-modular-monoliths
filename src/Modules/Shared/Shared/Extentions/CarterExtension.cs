using Carter;
using System.Reflection;

namespace Shared.Extentions
{
    public static class CarterExtension
    {
        public static IServiceCollection AddCarterWithAssemblies(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddCarter(configurator: config =>
            {
                foreach (var assembly in assemblies)
                {
                    var carterModules = assembly.GetTypes()
                        .Where(t => t.IsAssignableTo(typeof(ICarterModule))).ToArray();
                    config.WithModules(carterModules);
                }
            });
            return services;
        }
    }
}

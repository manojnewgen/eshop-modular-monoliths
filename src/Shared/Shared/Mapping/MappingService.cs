using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Shared.Mapping
{
    /// <summary>
    /// Mapster mapping service for dependency injection
    /// </summary>
    public interface IMappingService
    {
        TDestination Map<TDestination>(object source);
        TDestination Map<TSource, TDestination>(TSource source);
        TDestination Map<TDestination>(object source, TDestination destination);
        IQueryable<TDestination> ProjectToType<TDestination>(IQueryable source);
    }

    /// <summary>
    /// Implementation of mapping service using Mapster
    /// </summary>
    public class MappingService : IMappingService
    {
        private readonly TypeAdapterConfig _config;

        public MappingService(TypeAdapterConfig config)
        {
            _config = config;
        }

        public TDestination Map<TDestination>(object source)
        {
            return source.Adapt<TDestination>(_config);
        }

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            return source.Adapt<TDestination>(_config);
        }

        public TDestination Map<TDestination>(object source, TDestination destination)
        {
            return source.Adapt(destination, _config);
        }

        public IQueryable<TDestination> ProjectToType<TDestination>(IQueryable source)
        {
            return source.ProjectToType<TDestination>(_config);
        }
    }

    /// <summary>
    /// Extension methods for registering Mapster in DI container
    /// </summary>
    public static class MappingExtensions
    {
        public static IServiceCollection AddMapster(this IServiceCollection services, params Assembly[] assemblies)
        {
            var config = TypeAdapterConfig.GlobalSettings;
            
            // Register mapping configurations from assemblies
            var mappingConfigurations = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IMappingConfiguration).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                .Select(Activator.CreateInstance)
                .Cast<IMappingConfiguration>()
                .ToList();

            foreach (var mappingConfig in mappingConfigurations)
            {
                mappingConfig.Configure(config);
            }

            config.Compile();

            services.AddSingleton(config);
            services.AddScoped<IMappingService, MappingService>();

            return services;
        }

        public static IServiceCollection AddMapster(this IServiceCollection services, params Type[] assemblyMarkerTypes)
        {
            var assemblies = assemblyMarkerTypes.Select(t => t.Assembly).ToArray();
            return services.AddMapster(assemblies);
        }
    }
}
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messaging.Extensions
{
    public static class MassTransitExtension
    {
        public static IServiceCollection AddMassTransitWithAssemblies(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
        {
            services.AddMassTransit(config =>
            {
                config.SetKebabCaseEndpointNameFormatter();
                config.SetInMemorySagaRepositoryProvider();

                config.AddConsumers(assemblies);

                config.AddSagaStateMachines(assemblies);
                config.AddSagas(assemblies);
                config.AddActivities(assemblies);

                //config.UsingInMemory((context, configuarator) =>
                //{
                //    configuarator.ConfigureEndpoints(context);
                //});

                config.UsingRabbitMq((context, configurator) =>
                {
                    configurator.Host(configuration["EventBusSettings:HostAddress"], h =>
                    {
                        h.Username(configuration["EventBusSettings:UserName"]);
                        h.Password(configuration["EventBusSettings:Password"]);
                    });
                    configurator.ConfigureEndpoints(context);
                });

            });
            return services;
        }
    }
}

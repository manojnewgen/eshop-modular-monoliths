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
                    // Get RabbitMQ configuration from MessageBroker section (matches appsettings.json)
                    var hostAddress = configuration["MessageBroker:Host"];
                    var userName = configuration["MessageBroker:UserName"] ?? "guest";
                    var password = configuration["MessageBroker:Password"] ?? "guest";

                    // Check if host address is configured
                    if (string.IsNullOrEmpty(hostAddress))
                    {
                        // Fallback to default RabbitMQ connection for development
                        hostAddress = "amqp://localhost:5672";
                    }

                    configurator.Host(hostAddress, h =>
                    {
                        h.Username(userName);
                        h.Password(password);
                    });
                    configurator.ConfigureEndpoints(context);
                });

            });
            return services;
        }
    }
}

using System;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Topica;
using Topica.Contracts;
using Topica.Executors;
using Topica.RabbitMq.Clients;
using Topica.RabbitMq.Configuration;
using Topica.RabbitMq.Contracts;
using Topica.RabbitMq.Exchanges;
using Topica.RabbitMq.Queues;
using Topica.Resolvers;
using Topica.Topics;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class RabbitMqServiceExtensions
    {
        public static IServiceCollection AddRabbitMqTopica(this IServiceCollection services, Action<RabbitMqTopicaConfiguration> configuration)
        {
            var config = new RabbitMqTopicaConfiguration();
            configuration(config);

            var serviceProvider = services.BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<MessagingPlatform>>();
            logger.LogDebug("******* RabbitMq Service Extensions ******* ");

            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
            {
                throw new Exception($"{nameof(RabbitMqServiceExtensions)}: entry assembly is null, this can happen if the executing application is from unmanaged code");
            }

            services
                .AddHttpClient(nameof(RabbitMqManagementApiClient))
                .AddTypedClient<IRabbitMqManagementApiClient>(x =>
                {
                    x.BaseAddress = new Uri($"{config.ManagementScheme}{Uri.SchemeDelimiter}{config.Hostname}{(config.ManagementPort == null ? string.Empty : $":{config.ManagementPort}")}");
                    x.Timeout = TimeSpan.FromSeconds(2);
                    x.DefaultRequestHeaders.Add("Authorization", $"Basic {(string?)Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.UserName}:{config.Password}"))}");

                    return new RabbitMqManagementApiClient(config.VHost, x);
                });

            services.AddScoped<IConsumer, RabbitMqQueueConsumer>();
            services.AddScoped<ITopicCreatorFactory, TopicCreatorFactory>();
            services.AddScoped<ITopicCreator, RabbitMqExchangeCreator>();
            services.AddScoped<IHandlerResolver>(_ => new HandlerResolver(services.BuildServiceProvider(), entryAssembly));
            services.AddScoped<IMessageHandlerExecutor, MessageHandlerExecutor>();
            services.AddSingleton(_ => new ConnectionFactory
            {
                Uri = new Uri($"{config.Scheme}{Uri.SchemeDelimiter}{config.UserName}:{config.Password}@{config.Hostname}:{config.Port}/{config.VHost}"),
                DispatchConsumersAsync = true,
                RequestedHeartbeat = TimeSpan.FromSeconds(10),
                AutomaticRecoveryEnabled = true
            });

            // Scan for IHandlers from Entry assembly
            services.Scan(s => s
                .FromAssemblies(entryAssembly!)
                .AddClasses(c => c.AssignableTo(typeof(IHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());


            return services;
        }
    }
}
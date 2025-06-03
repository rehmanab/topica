using System;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Topica;
using Topica.Contracts;
using Topica.Executors;
using Topica.RabbitMq.Builders;
using Topica.RabbitMq.Clients;
using Topica.RabbitMq.Configuration;
using Topica.RabbitMq.Contracts;
using Topica.RabbitMq.Providers;
using Topica.Resolvers;
using Topica.Topics;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class RabbitMqServiceExtensions
    {
        public static IServiceCollection AddRabbitMqTopica(this IServiceCollection services, Action<RabbitMqTopicaConfiguration> configurationFactory, Assembly assembly)
        {
            if (assembly == null)
            {
                throw new Exception($"{nameof(RabbitMqServiceExtensions)}: entry assembly is null, this can happen if the executing application is from unmanaged code");
            }
            
            var config = new RabbitMqTopicaConfiguration();
            configurationFactory(config);

            var serviceProvider = services.BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<MessagingPlatform>>();
            
            if (logger == null)
            {
                throw new Exception($"{nameof(RabbitMqServiceExtensions)}: logger is null, this can happen if the executing application is from unmanaged code");
            }
            
            logger.LogDebug("******* RabbitMq Service Extensions ******* ");

            services
                .AddHttpClient(nameof(RabbitMqManagementApiClient))
                .AddTypedClient<IRabbitMqManagementApiClient>(x =>
                {
                    x.BaseAddress = new Uri($"{config.ManagementScheme}{Uri.SchemeDelimiter}{config.Hostname}{(config.ManagementPort == null ? string.Empty : $":{config.ManagementPort}")}");
                    x.Timeout = TimeSpan.FromSeconds(2);
                    x.DefaultRequestHeaders.Add("Authorization", $"Basic {(string?)Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.UserName}:{config.Password}"))}");

                    return new RabbitMqManagementApiClient(config.VHost, x);
                });

            services.AddScoped<IRabbitMqTopicFluentBuilder, RabbitMqTopicFluentBuilder>();
            services.AddScoped<ITopicProviderFactory, TopicProviderFactory>();
            services.AddScoped<ITopicProvider, RabbitMqExchangeProvider>();
            services.AddScoped<IHandlerResolver>(_ => new HandlerResolver(services.BuildServiceProvider(), assembly, logger));
            services.AddScoped<IMessageHandlerExecutor, MessageHandlerExecutor>();
            services.AddSingleton(_ => new ConnectionFactory
            {
                Uri = new Uri($"{config.Scheme}{Uri.SchemeDelimiter}{config.UserName}:{config.Password}@{config.Hostname}:{config.Port}/{config.VHost}"),
                RequestedHeartbeat = TimeSpan.FromSeconds(10),
                AutomaticRecoveryEnabled = true
            });

            // Scan for IHandlers from Entry assembly
            services.Scan(s => s
                .FromAssemblies(assembly)
                .AddClasses(c => c.AssignableTo(typeof(IHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
            
            return services;
        }
    }
}
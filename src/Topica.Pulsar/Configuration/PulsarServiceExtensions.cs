using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Pulsar.Client.Api;
using Topica;
using Topica.Contracts;
using Topica.Executors;
using Topica.Pulsar.Configuration;
using Topica.Pulsar.Consumers;
using Topica.Pulsar.Contracts;
using Topica.Pulsar.Producers;
using Topica.Pulsar.Providers;
using Topica.Pulsar.Services;
using Topica.Resolvers;
using Topica.Services;
using Topica.Topics;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class PulsarServiceExtensions
    {
        public static IServiceCollection AddPulsarTopica(this IServiceCollection services, Action<PulsarTopicaConfiguration> configurationFactory)
        {
            var config = new PulsarTopicaConfiguration();
            configurationFactory(config);
            
            var serviceProvider = services.BuildServiceProvider();
            
            var logger = serviceProvider.GetService<ILogger<MessagingPlatform>>();
            logger.LogDebug("******* Pulsar Service Extensions ******* ");

            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
            {
                throw new Exception($"{nameof(PulsarServiceExtensions)}: entry assembly is null, this can happen if the executing application is from unmanaged code");
            }

            services.AddHttpClient<IHttpClientService, HttpClientService>();
            services.AddScoped<IPulsarService, PulsarService>(provider =>
            {
                var httpClientService = provider.GetService<IHttpClientService>();
                var logger = provider.GetService<ILogger<PulsarService>>();
                
                return new PulsarService(config.PulsarManagerBaseUrl, config.PulsarAdminBaseUrl, httpClientService, logger);
            });
            services.AddScoped<IConsumer, PulsarTopicConsumer>();
            services.AddScoped<IProducerBuilder, PulsarTopicProducerBuilder>();
            services.AddScoped<ITopicProviderFactory, TopicProviderFactory>();
            services.AddScoped<ITopicProvider, PulsarTopicProvider>();
            services.AddScoped<IHandlerResolver>(_ => new HandlerResolver(services.BuildServiceProvider(), entryAssembly));
            services.AddTransient<IMessageHandlerExecutor, MessageHandlerExecutor>();
            services.AddSingleton(_ => new PulsarClientBuilder().ServiceUrl(config.ServiceUrl));
            
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
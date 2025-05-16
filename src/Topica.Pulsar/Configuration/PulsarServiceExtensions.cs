using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Pulsar.Client.Api;
using Topica;
using Topica.Contracts;
using Topica.Executors;
using Topica.Pulsar.Builders;
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
        public static IServiceCollection AddPulsarTopica(this IServiceCollection services, Action<PulsarTopicaConfiguration> configurationFactory, Assembly assembly)
        {
            var config = new PulsarTopicaConfiguration();
            configurationFactory(config);
            
            var serviceProvider = services.BuildServiceProvider();
            
            var logger = serviceProvider.GetService<ILogger<MessagingPlatform>>();
            
            if (logger == null)
            {
                throw new Exception($"{nameof(PulsarServiceExtensions)}: logger is null, this can happen if the executing application is from unmanaged code");
            }
            
            logger.LogDebug("******* Pulsar Service Extensions ******* ");

            services.AddHttpClient<IHttpClientService, HttpClientService>();
            services.AddScoped<IPulsarService, PulsarService>(provider =>
            {
                var httpClientService = provider.GetService<IHttpClientService>();
                var localLogger = provider.GetService<ILogger<PulsarService>>();
                
                if (httpClientService == null)
                {
                    throw new Exception($"{nameof(PulsarServiceExtensions)}: httpClientService is null, this can happen if the executing application is from unmanaged code");
                }
                
                if (localLogger == null)
                {
                    throw new Exception($"{nameof(PulsarServiceExtensions)}: logger is null, this can happen if the executing application is from unmanaged code");
                }
                
                return new PulsarService(config.PulsarManagerBaseUrl!, config.PulsarAdminBaseUrl!, httpClientService, localLogger);
            });
            services.AddScoped<IConsumer, PulsarTopicConsumer>();
            services.AddScoped<IPulsarConsumerTopicFluentBuilder, PulsarConsumerTopicFluentBuilder>();
            services.AddScoped<IProducerBuilder, PulsarTopicProducerBuilder>();
            services.AddScoped<ITopicProviderFactory, TopicProviderFactory>();
            services.AddScoped<ITopicProvider, PulsarTopicProvider>();
            services.AddScoped<IHandlerResolver>(_ => new HandlerResolver(services.BuildServiceProvider(), assembly));
            services.AddTransient<IMessageHandlerExecutor, MessageHandlerExecutor>();
            services.AddSingleton(_ => new PulsarClientBuilder().ServiceUrl(config.ServiceUrl));
            
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
using System;
using System.Reflection;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Topica;
using Topica.Contracts;
using Topica.Executors;
using Topica.Kafka.Builders;
using Topica.Kafka.Configuration;
using Topica.Kafka.Contracts;
using Topica.Kafka.Providers;
using Topica.Resolvers;
using Topica.Services;
using Topica.Topics;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class KafkaServiceExtensions
    {
        public static IServiceCollection AddKafkaTopica(this IServiceCollection services, Action<KafkaTopicaConfiguration> configurationFactory, Assembly assembly)
        {
            if (assembly == null)
            {
                throw new Exception($"{nameof(KafkaServiceExtensions)}: entry assembly is null, this can happen if the executing application is from unmanaged code");
            }
            
            var config = new KafkaTopicaConfiguration();
            configurationFactory(config);
            
            var serviceProvider = services.BuildServiceProvider();
            
            var logger = serviceProvider.GetService<ILogger<MessagingPlatform>>();
            if (logger == null)
            {
                throw new Exception($"{nameof(KafkaServiceExtensions)}: logger is null, this can happen if the executing application is from unmanaged code");
            }
            
            services.AddScoped<IPollyRetryService, PollyRetryService>();
            services.AddScoped<IKafkaTopicCreationBuilder, KafkaTopicCreationBuilder>();
            services.AddScoped<ITopicProviderFactory, TopicProviderFactory>();
            services.AddScoped<IAdminClient>(_ => new AdminClientBuilder(new AdminClientConfig { BootstrapServers = string.Join(",", config.BootstrapServers) }).Build());
            services.AddScoped<ITopicProvider, KafkaTopicProvider>();
            services.AddScoped<IHandlerResolver>(_ => new HandlerResolver(services.BuildServiceProvider(), assembly, logger));
            services.AddTransient<IMessageHandlerExecutor, MessageHandlerExecutor>();
            
            // Scan for IHandlers from assembly
            services.Scan(s => s
                .FromAssemblies(assembly)
                .AddClasses(c => c.AssignableTo(typeof(IHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());


            return services;
        }
    }
}
using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Topica;
using Topica.Contracts;
using Topica.Executors;
using Topica.Kafka.Builders;
using Topica.Kafka.Consumers;
using Topica.Kafka.Contracts;
using Topica.Kafka.Producers;
using Topica.Kafka.Providers;
using Topica.Resolvers;
using Topica.Topics;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class KafkaServiceExtensions
    {
        public static IServiceCollection AddKafkaTopica(this IServiceCollection services, Assembly assembly)
        {
            if (assembly == null)
            {
                throw new Exception($"{nameof(KafkaServiceExtensions)}: entry assembly is null, this can happen if the executing application is from unmanaged code");
            }
            
            var serviceProvider = services.BuildServiceProvider();
            
            var logger = serviceProvider.GetService<ILogger<MessagingPlatform>>();
            if (logger == null)
            {
                throw new Exception($"{nameof(KafkaServiceExtensions)}: logger is null, this can happen if the executing application is from unmanaged code");
            }
            logger.LogDebug("******* Kafka Service Extensions ******* ");
            
            services.AddScoped<IConsumer, KafkaTopicConsumer>();
            services.AddScoped<IKafkaConsumerTopicFluentBuilder, KafkaConsumerTopicFluentBuilder>();
            services.AddScoped<IProducerBuilder, KafkaProducerBuilder>();
            services.AddScoped<ITopicProviderFactory, TopicProviderFactory>();
            services.AddScoped<ITopicProvider, KafkaTopicProvider>();
            services.AddScoped<IHandlerResolver>(_ => new HandlerResolver(services.BuildServiceProvider(), assembly));
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
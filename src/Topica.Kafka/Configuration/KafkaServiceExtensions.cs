using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Topica;
using Topica.Contracts;
using Topica.Executors;
using Topica.Kafka.Settings;
using Topica.Kafka.Topics;
using Topica.Resolvers;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class KafkaServiceExtensions
    {
        public static IServiceCollection AddKafkaTopica(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            
            var logger = serviceProvider.GetService<ILogger<MessagingPlatform>>();
            logger.LogDebug("******* Kafka Service Extensions ******* ");
            
            var kafkaSettings = serviceProvider.GetService<KafkaSettings>();

            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
            {
                throw new Exception($"{nameof(KafkaServiceExtensions)}: entry assembly is null, this can happen if the executing application is from unmanaged code");
            }
            
            services.AddScoped<IConsumer, KafkaTopicConsumer>();
            services.AddScoped<IHandlerResolver>(_ => new HandlerResolver(services.BuildServiceProvider(), entryAssembly));
            services.AddTransient<IMessageHandlerExecutor, MessageHandlerExecutor>();
            
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
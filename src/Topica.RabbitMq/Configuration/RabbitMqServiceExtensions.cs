using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Topica;
using Topica.Contracts;
using Topica.Executors;
using Topica.RabbitMq.Exchanges;
using Topica.RabbitMq.Queues;
using Topica.RabbitMq.Services;
using Topica.RabbitMq.Settings;
using Topica.Resolvers;
using Topica.Topics;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class RabbitMqServiceExtensions
    {
        public static IServiceCollection AddRabbitMqTopica(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            
            var logger = serviceProvider.GetService<ILogger<MessagingPlatform>>();
            logger.LogDebug("******* RabbitMq Service Extensions ******* ");
            
            var rabbitMqSettings = serviceProvider.GetService<RabbitMqSettings>();

            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
            {
                throw new Exception($"{nameof(RabbitMqServiceExtensions)}: entry assembly is null, this can happen if the executing application is from unmanaged code");
            }

            services.AddHttpClient<IRabbitMqManagementService, RabbitMqManagementService>(x =>
            {
                x.BaseAddress = new Uri($"{rabbitMqSettings.ManagementScheme}{Uri.SchemeDelimiter}{rabbitMqSettings.Hostname}{(rabbitMqSettings.ManagementPort == null ? string.Empty : $":{rabbitMqSettings.ManagementPort}")}");
            });
            services.AddScoped<IConsumer, RabbitMqQueueConsumer>();
            services.AddScoped<ITopicCreatorFactory, TopicCreatorFactory>();
            services.AddScoped<ITopicCreator, RabbitMqExchangeCreator>();
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
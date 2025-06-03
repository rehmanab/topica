using Microsoft.Extensions.Logging;
using Topica;
using Topica.Azure.ServiceBus.Configuration;
using Topica.Contracts;
using Topica.Executors;
using Topica.Resolvers;
using Topica.Topics;
using System.Reflection;
using Topica.Azure.ServiceBus.Builders;
using Topica.Azure.ServiceBus.Contracts;
using Topica.Azure.ServiceBus.Providers;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class AzureServiceBusExtensions
{
    public static IServiceCollection AddAzureServiceBusTopica(this IServiceCollection services, Action<AzureServiceBusTopicaConfiguration> configuration, Assembly assembly)
    {
        if (assembly == null)
        {
            throw new Exception($"{nameof(AzureServiceBusExtensions)}: entry assembly is null, this can happen if the executing application is from unmanaged code");
        }
        
        var config = new AzureServiceBusTopicaConfiguration();
        configuration(config);
            
        var serviceProvider = services.BuildServiceProvider();
            
        var logger = serviceProvider.GetService<ILogger<MessagingPlatform>>();
            
        if (logger == null)
        {
            throw new Exception($"{nameof(AzureServiceBusExtensions)}: logger is null, this can happen if the executing application is from unmanaged code");
        }
            
        logger.LogDebug("******* AzureServiceBus Service Extensions ******* ");

        services.AddScoped<IServiceBusAdministrationClientProvider>(_ => new ServiceBusAdministrationClientProvider(config.ConnectionString!));
        services.AddScoped<IServiceBusClientProvider>(_ => new ServiceBusClientProvider(config.ConnectionString!));
        services.AddScoped<IAzureServiceBusTopicFluentBuilder, AzureServiceBusTopicFluentBuilder>();
        services.AddScoped<ITopicProviderFactory, TopicProviderFactory>();
        services.AddScoped<ITopicProvider, AzureServiceBusTopicProvider>();
        services.AddScoped<IHandlerResolver>(_ => new HandlerResolver(services.BuildServiceProvider(), assembly, logger));
        services.AddTransient<IMessageHandlerExecutor, MessageHandlerExecutor>();
            
        // Scan for IHandlers from Entry assembly
        services.Scan(s => s
            .FromAssemblies(assembly!)
            .AddClasses(c => c.AssignableTo(typeof(IHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
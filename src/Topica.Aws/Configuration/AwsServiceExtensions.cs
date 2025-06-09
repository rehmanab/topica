using System;
using System.Reflection;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Microsoft.Extensions.Logging;
using Topica;
using Topica.Aws.Builders;
using Topica.Aws.Configuration;
using Topica.Aws.Contracts;
using Topica.Aws.Providers;
using Topica.Aws.Services;
using Topica.Contracts;
using Topica.Executors;
using Topica.Infrastructure.Contracts;
using Topica.Infrastructure.Services;
using Topica.Resolvers;
using Topica.Topics;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class AwsServiceExtensions
{
    public static IServiceCollection AddAwsTopica(this IServiceCollection services, Action<AwsTopicaConfiguration> configuration, Assembly assembly)
    {
        if (assembly == null)
        {
            throw new Exception($"{nameof(AwsServiceExtensions)}: entry assembly is null, this can happen if the executing application is from unmanaged code");
        }
        
        var config = new AwsTopicaConfiguration();
        configuration(config);
            
        var serviceProvider = services.BuildServiceProvider();
            
        var logger = serviceProvider.GetService<ILogger<MessagingPlatform>>();
            
        if (logger == null)
        {
            throw new Exception($"{nameof(AwsServiceExtensions)}: logger is null, this can happen if the executing application is from unmanaged code");
        }
            
        var awsClientService = new AwsClientService(logger);
        services.AddScoped<IPollyRetryService, PollyRetryService>();
        services.AddScoped<IAwsClientService>(_ => awsClientService);
        services.AddScoped<IAmazonSimpleNotificationService>(_ => awsClientService.GetSnsClient(config));
        services.AddScoped<IAmazonSQS>(_ => awsClientService.GetSqsClient(config));
        services.AddScoped<IAwsQueueService, AwsQueueService>();
        services.AddScoped<IAwsTopicService, AwsTopicService>();
        services.AddScoped<IAwsQueueCreationBuilder, AwsQueueCreationBuilder>();
        services.AddScoped<IAwsTopicCreationBuilder, AwsTopicCreationBuilder>();
        services.AddScoped<IQueueProviderFactory, QueueProviderFactory>();
        services.AddScoped<ITopicProviderFactory, TopicProviderFactory>();
        services.AddScoped<IQueueProvider, AwsQueueProvider>();
        services.AddScoped<ITopicProvider, AwsTopicProvider>();
        services.AddScoped<IHandlerResolver>(_ => new HandlerResolver(services.BuildServiceProvider(), assembly, logger));
        services.AddTransient<IMessageHandlerExecutor, MessageHandlerExecutor>();
            
        // Scan for IHandlers from Entry assembly
        services.Scan(s => s
            .FromAssemblies(assembly)
            .AddClasses(c => c.AssignableTo(typeof(IHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
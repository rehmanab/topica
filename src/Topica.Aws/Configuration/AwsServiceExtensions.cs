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
using Topica.Resolvers;
using Topica.Services;
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
        services.AddTransient<IPollyRetryService, PollyRetryService>();
        services.AddTransient<IAwsClientService>(_ => awsClientService);
        services.AddTransient<IAmazonSimpleNotificationService>(_ => awsClientService.GetSnsClient(config));
        services.AddTransient<IAmazonSQS>(_ => awsClientService.GetSqsClient(config));
        services.AddTransient<IAwsQueueService, AwsQueueService>();
        services.AddTransient<IAwsTopicService, AwsTopicService>();
        services.AddTransient<IAwsQueueCreationBuilder, AwsQueueCreationBuilder>();
        services.AddTransient<IAwsTopicCreationBuilder, AwsTopicCreationBuilder>();
        services.AddTransient<IQueueProviderFactory, QueueProviderFactory>();
        services.AddTransient<ITopicProviderFactory, TopicProviderFactory>();
        services.AddTransient<IQueueProvider, AwsQueueProvider>();
        services.AddTransient<ITopicProvider, AwsTopicProvider>();
        services.AddTransient<IHandlerResolver>(_ => new HandlerResolver(services.BuildServiceProvider(), assembly, logger));
        services.AddTransient<IMessageHandlerExecutor, MessageHandlerExecutor>();
            
        // Scan for IHandlers from Entry assembly
        services.Scan(s => s
            .FromAssemblies(assembly)
            .AddClasses(c => c.AssignableTo(typeof(IHandler<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        return services;
    }
}
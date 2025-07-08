using System.Reflection;
using Aws.Topic.Consumer.Host;
using Aws.Topic.Consumer.Host.Settings;
using Aws.Topic.Consumer.Host.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Aws.Contracts;
using Topica.Contracts;
using Topica.SharedMessageHandlers;

Console.WriteLine("******* Starting Aws.Topic.Consumer.Host *******");

var host = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration(builder =>
        {
            builder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .AddEnvironmentVariables();
        }
    )
    .ConfigureServices((ctx, services) =>
    {
        // Configuration
        var hostSettings = ctx.Configuration.GetSection(AwsHostSettings.SectionName).Get<AwsHostSettings>();
        var settings = ctx.Configuration.GetSection(AwsConsumerSettings.SectionName).Get<AwsConsumerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(AwsHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (settings == null) throw new InvalidOperationException($"{nameof(AwsConsumerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new AwsHostSettingsValidator().ValidateAndThrow(hostSettings);
        new AwsConsumerSettingsValidator().ValidateAndThrow(settings);

        services.AddSingleton(hostSettings);
        services.AddSingleton(settings);

        services.AddLogging(configure => configure
            .AddSimpleConsole(x =>
            {
                x.IncludeScopes = false;
                x.TimestampFormat = "[HH:mm:ss] ";
                x.SingleLine = true;
            })
            .AddSeq(ctx.Configuration.GetSection(SeqSettings.SectionName)));

        // Add MessagingPlatform Components
        services.AddAwsTopica(c =>
        {
            c.ProfileName = hostSettings.ProfileName;
            c.AccessKey = hostSettings.AccessKey;
            c.SecretKey = hostSettings.SecretKey;
            c.ServiceUrl = hostSettings.ServiceUrl;
            c.RegionEndpoint = hostSettings.RegionEndpoint;
        }, Assembly.GetAssembly(typeof(ClassToReferenceAssembly)) ?? throw new InvalidOperationException());
        // Assembly.GetExecutingAssembly()

        services.AddHostedService<Worker>();

        AddCreationProducer(services, settings);
        // AddNonCreationProducer(services, settings);
    })
    .Build();

await host.RunAsync();
return;

void AddCreationProducer(IServiceCollection serviceCollection, AwsConsumerSettings awsConsumerSettings)
{
    serviceCollection.AddKeyedSingleton<IConsumer>("Consumer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IAwsTopicCreationBuilder>()
        .WithWorkerName(awsConsumerSettings.WebAnalyticsTopicSettings.WorkerName)
        .WithTopicName(awsConsumerSettings.WebAnalyticsTopicSettings.Source)
        .WithSubscribedQueues(awsConsumerSettings.WebAnalyticsTopicSettings.WithSubscribedQueues)
        .WithQueueToSubscribeTo(awsConsumerSettings.WebAnalyticsTopicSettings.SubscribeToSource)
        .WithErrorQueueSettings(
            awsConsumerSettings.WebAnalyticsTopicSettings.BuildWithErrorQueue,
            awsConsumerSettings.WebAnalyticsTopicSettings.ErrorQueueMaxReceiveCount
        )
        .WithFifoSettings(
            awsConsumerSettings.WebAnalyticsTopicSettings.IsFifoQueue,
            awsConsumerSettings.WebAnalyticsTopicSettings.IsFifoContentBasedDeduplication
        )
        .WithTemporalSettings(
            awsConsumerSettings.WebAnalyticsTopicSettings.MessageVisibilityTimeoutSeconds,
            awsConsumerSettings.WebAnalyticsTopicSettings.QueueMessageDelaySeconds,
            awsConsumerSettings.WebAnalyticsTopicSettings.QueueMessageRetentionPeriodSeconds,
            awsConsumerSettings.WebAnalyticsTopicSettings.QueueReceiveMessageWaitTimeSeconds
        )
        .WithQueueSettings(awsConsumerSettings.WebAnalyticsTopicSettings.QueueMaximumMessageSize)
        .WithConsumeSettings(
            awsConsumerSettings.WebAnalyticsTopicSettings.NumberOfInstances,
            awsConsumerSettings.WebAnalyticsTopicSettings.QueueReceiveMaximumNumberOfMessages
        ).BuildConsumerAsync(CancellationToken.None).Result);
}

void AddNonCreationProducer(IServiceCollection serviceCollection, AwsConsumerSettings awsConsumerSettings)
{
    serviceCollection.AddKeyedSingleton<IConsumer>("Consumer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IAwsTopicBuilder>()
        .WithWorkerName(awsConsumerSettings.WebAnalyticsTopicSettings.WorkerName)
        .WithTopicName(awsConsumerSettings.WebAnalyticsTopicSettings.Source)
        .WithQueueToSubscribeTo(awsConsumerSettings.WebAnalyticsTopicSettings.SubscribeToSource)
        .WithFifoSettings(
            awsConsumerSettings.WebAnalyticsTopicSettings.IsFifoQueue,
            awsConsumerSettings.WebAnalyticsTopicSettings.IsFifoContentBasedDeduplication
        )
        .WithConsumeSettings(
            awsConsumerSettings.WebAnalyticsTopicSettings.NumberOfInstances,
            awsConsumerSettings.WebAnalyticsTopicSettings.QueueReceiveMaximumNumberOfMessages
        ).BuildConsumerAsync(CancellationToken.None).Result);
}
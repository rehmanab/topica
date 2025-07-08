using System.Reflection;
using Aws.Queue.Consumer.Host;
using Aws.Queue.Consumer.Host.Settings;
using Aws.Queue.Consumer.Host.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Aws.Contracts;
using Topica.Contracts;
using Topica.SharedMessageHandlers;

Console.WriteLine("******* Starting Aws.Queue.Consumer.Host *******");

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

        AddCreationConsumer(services, settings);
        // AddNonCreationConsumer(services, settings);
    })
    .Build();

await host.RunAsync();
return;

void AddCreationConsumer(IServiceCollection serviceCollection, AwsConsumerSettings awsConsumerSettings)
{
    serviceCollection.AddKeyedSingleton<IConsumer>("Consumer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IAwsQueueCreationBuilder>()
        .WithWorkerName(awsConsumerSettings.WebAnalyticsQueueSettings.WorkerName)
        .WithQueueName(awsConsumerSettings.WebAnalyticsQueueSettings.Source)
        .WithErrorQueueSettings(
            awsConsumerSettings.WebAnalyticsQueueSettings.BuildWithErrorQueue,
            awsConsumerSettings.WebAnalyticsQueueSettings.ErrorQueueMaxReceiveCount
        )
        .WithFifoSettings(
            awsConsumerSettings.WebAnalyticsQueueSettings.IsFifoQueue,
            awsConsumerSettings.WebAnalyticsQueueSettings.IsFifoContentBasedDeduplication
        )
        .WithTemporalSettings(
            awsConsumerSettings.WebAnalyticsQueueSettings.MessageVisibilityTimeoutSeconds,
            awsConsumerSettings.WebAnalyticsQueueSettings.QueueMessageDelaySeconds,
            awsConsumerSettings.WebAnalyticsQueueSettings.QueueMessageRetentionPeriodSeconds,
            awsConsumerSettings.WebAnalyticsQueueSettings.QueueReceiveMessageWaitTimeSeconds
        )
        .WithQueueSettings(awsConsumerSettings.WebAnalyticsQueueSettings.QueueMaximumMessageSize)
        .WithConsumeSettings(
            awsConsumerSettings.WebAnalyticsQueueSettings.NumberOfInstances,
            awsConsumerSettings.WebAnalyticsQueueSettings.QueueReceiveMaximumNumberOfMessages
        ).BuildConsumerAsync(CancellationToken.None).Result);
}

void AddNonCreationConsumer(IServiceCollection serviceCollection, AwsConsumerSettings awsConsumerSettings)
{
    serviceCollection.AddKeyedSingleton<IConsumer>("Consumer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IAwsQueueBuilder>()
        .WithWorkerName(awsConsumerSettings.WebAnalyticsQueueSettings.WorkerName)
        .WithQueueName(awsConsumerSettings.WebAnalyticsQueueSettings.Source)
        .WithFifoSettings(
            awsConsumerSettings.WebAnalyticsQueueSettings.IsFifoQueue,
            awsConsumerSettings.WebAnalyticsQueueSettings.IsFifoContentBasedDeduplication
        )
        .BuildConsumerAsync(CancellationToken.None).Result);
}
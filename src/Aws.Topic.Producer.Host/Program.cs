using System.Reflection;
using Aws.Topic.Producer.Host;
using Aws.Topic.Producer.Host.Settings;
using Aws.Topic.Producer.Host.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Aws.Contracts;
using Topica.Contracts;

Console.WriteLine("******* Starting Aws.Topic.Producer.Host *******");

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
        var settings = ctx.Configuration.GetSection(AwsProducerSettings.SectionName).Get<AwsProducerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(AwsHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (settings == null) throw new InvalidOperationException($"{nameof(AwsProducerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new AwsHostSettingsValidator().ValidateAndThrow(hostSettings);
        new AwsProducerSettingsValidator().ValidateAndThrow(settings);

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
        }, Assembly.GetExecutingAssembly());

        services.Configure<HostOptions>(options => { options.ShutdownTimeout = TimeSpan.FromSeconds(5); });

        services.AddHostedService<Worker>();

        AddCreationProducer(services, settings);
        // AddNonCreationProducer(services, settings);
    })
    .Build();

await host.RunAsync();
return;

void AddCreationProducer(IServiceCollection serviceCollection, AwsProducerSettings awsProducerSettings)
{
    serviceCollection.AddKeyedSingleton<IProducer>("Producer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IAwsTopicCreationBuilder>()
        .WithWorkerName(awsProducerSettings.WebAnalyticsTopicSettings.WorkerName)
        .WithTopicName(awsProducerSettings.WebAnalyticsTopicSettings.Source)
        .WithSubscribedQueues(awsProducerSettings.WebAnalyticsTopicSettings.WithSubscribedQueues)
        .WithQueueToSubscribeTo(awsProducerSettings.WebAnalyticsTopicSettings.SubscribeToSource)
        .WithErrorQueueSettings(
            awsProducerSettings.WebAnalyticsTopicSettings.BuildWithErrorQueue,
            awsProducerSettings.WebAnalyticsTopicSettings.ErrorQueueMaxReceiveCount
        )
        .WithFifoSettings(
            awsProducerSettings.WebAnalyticsTopicSettings.IsFifoQueue,
            awsProducerSettings.WebAnalyticsTopicSettings.IsFifoContentBasedDeduplication
        )
        .WithTemporalSettings(
            awsProducerSettings.WebAnalyticsTopicSettings.MessageVisibilityTimeoutSeconds,
            awsProducerSettings.WebAnalyticsTopicSettings.QueueMessageDelaySeconds,
            awsProducerSettings.WebAnalyticsTopicSettings.QueueMessageRetentionPeriodSeconds,
            awsProducerSettings.WebAnalyticsTopicSettings.QueueReceiveMessageWaitTimeSeconds
        )
        .WithQueueSettings(awsProducerSettings.WebAnalyticsTopicSettings.QueueMaximumMessageSize)
        .BuildProducerAsync(CancellationToken.None).Result);
}

void AddNonCreationProducer(IServiceCollection serviceCollection, AwsProducerSettings awsProducerSettings)
{
    serviceCollection.AddKeyedSingleton<IProducer>("Producer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IAwsTopicBuilder>()
        .WithWorkerName(awsProducerSettings.WebAnalyticsTopicSettings.WorkerName)
        .WithTopicName(awsProducerSettings.WebAnalyticsTopicSettings.Source)
        .WithQueueToSubscribeTo(awsProducerSettings.WebAnalyticsTopicSettings.SubscribeToSource)
        .WithFifoSettings(
            awsProducerSettings.WebAnalyticsTopicSettings.IsFifoQueue,
            awsProducerSettings.WebAnalyticsTopicSettings.IsFifoContentBasedDeduplication
        )
        .BuildProducerAsync(CancellationToken.None).Result);
}
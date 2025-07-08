using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pulsar.Topic.Producer.Host;
using Pulsar.Topic.Producer.Host.Settings;
using Pulsar.Topic.Producer.Host.Validators;
using Topica.Contracts;
using Topica.Pulsar.Contracts;

Console.WriteLine("******* Starting Pulsar.Topic.Producer.Host *******");

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
        var hostSettings = ctx.Configuration.GetSection(PulsarHostSettings.SectionName).Get<PulsarHostSettings>();
        var settings = ctx.Configuration.GetSection(PulsarProducerSettings.SectionName).Get<PulsarProducerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(PulsarHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (settings == null) throw new InvalidOperationException($"{nameof(PulsarProducerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new PulsarHostSettingsValidator().ValidateAndThrow(hostSettings);
        new PulsarProducerSettingsValidator().ValidateAndThrow(settings);

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
        services.AddPulsarTopica(c =>
        {
            c.ServiceUrl = hostSettings.ServiceUrl;
            c.PulsarManagerBaseUrl = hostSettings.PulsarManagerBaseUrl;
            c.PulsarAdminBaseUrl = hostSettings.PulsarAdminBaseUrl;
        }, Assembly.GetExecutingAssembly());

        services.Configure<HostOptions>(options => { options.ShutdownTimeout = TimeSpan.FromSeconds(5); });

        services.AddHostedService<Worker>();

        AddCreationProducer(services, settings);
        // AddNonCreationProducer(services, settings);
    })
    .Build();

await host.RunAsync();
return;

void AddCreationProducer(IServiceCollection serviceCollection, PulsarProducerSettings pulsarProducerSettings)
{
    serviceCollection.AddKeyedSingleton<IProducer>("Producer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IPulsarTopicCreationBuilder>()
        .WithWorkerName(pulsarProducerSettings.WebAnalyticsTopicSettings.WorkerName)
        .WithTopicName(pulsarProducerSettings.WebAnalyticsTopicSettings.Source)
        .WithConsumerGroup(pulsarProducerSettings.WebAnalyticsTopicSettings.ConsumerGroup)
        .WithConfiguration(
            pulsarProducerSettings.WebAnalyticsTopicSettings.Tenant,
            pulsarProducerSettings.WebAnalyticsTopicSettings.Namespace,
            pulsarProducerSettings.WebAnalyticsTopicSettings.NumberOfPartitions
        )
        .WithTopicOptions(pulsarProducerSettings.WebAnalyticsTopicSettings.StartNewConsumerEarliest)
        .WithProducerOptions(
            pulsarProducerSettings.WebAnalyticsTopicSettings.BlockIfQueueFull,
            pulsarProducerSettings.WebAnalyticsTopicSettings.MaxPendingMessages,
            pulsarProducerSettings.WebAnalyticsTopicSettings.MaxPendingMessagesAcrossPartitions,
            pulsarProducerSettings.WebAnalyticsTopicSettings.EnableBatching,
            pulsarProducerSettings.WebAnalyticsTopicSettings.EnableChunking,
            pulsarProducerSettings.WebAnalyticsTopicSettings.BatchingMaxMessages,
            pulsarProducerSettings.WebAnalyticsTopicSettings.BatchingMaxPublishDelayMilliseconds
        ).BuildProducerAsync(CancellationToken.None).Result);
}

void AddNonCreationProducer(IServiceCollection serviceCollection, PulsarProducerSettings pulsarProducerSettings)
{
    serviceCollection.AddKeyedSingleton<IProducer>("Producer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IPulsarTopicBuilder>()
        .WithWorkerName(pulsarProducerSettings.WebAnalyticsTopicSettings.WorkerName)
        .WithTopicName(pulsarProducerSettings.WebAnalyticsTopicSettings.Source)
        .WithConsumerGroup(pulsarProducerSettings.WebAnalyticsTopicSettings.ConsumerGroup)
        .WithConfiguration(
            pulsarProducerSettings.WebAnalyticsTopicSettings.Tenant,
            pulsarProducerSettings.WebAnalyticsTopicSettings.Namespace
        )
        .WithTopicOptions(pulsarProducerSettings.WebAnalyticsTopicSettings.StartNewConsumerEarliest)
        .WithProducerOptions(
            pulsarProducerSettings.WebAnalyticsTopicSettings.BlockIfQueueFull,
            pulsarProducerSettings.WebAnalyticsTopicSettings.MaxPendingMessages,
            pulsarProducerSettings.WebAnalyticsTopicSettings.MaxPendingMessagesAcrossPartitions,
            pulsarProducerSettings.WebAnalyticsTopicSettings.EnableBatching,
            pulsarProducerSettings.WebAnalyticsTopicSettings.EnableChunking,
            pulsarProducerSettings.WebAnalyticsTopicSettings.BatchingMaxMessages,
            pulsarProducerSettings.WebAnalyticsTopicSettings.BatchingMaxPublishDelayMilliseconds
        ).BuildProducerAsync(CancellationToken.None).Result);
}
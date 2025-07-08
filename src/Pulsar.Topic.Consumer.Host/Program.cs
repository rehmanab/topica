using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pulsar.Topic.Consumer.Host;
using Pulsar.Topic.Consumer.Host.Settings;
using Pulsar.Topic.Consumer.Host.Validators;
using Topica.Contracts;
using Topica.SharedMessageHandlers;
using Topica.Pulsar.Contracts;

Console.WriteLine("******* Starting Pulsar.Topic.Consumer.Host *******");

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
        var settings = ctx.Configuration.GetSection(PulsarConsumerSettings.SectionName).Get<PulsarConsumerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(PulsarHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (settings == null) throw new InvalidOperationException($"{nameof(PulsarConsumerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new PulsarHostSettingsValidator().ValidateAndThrow(hostSettings);
        new PulsarConsumerSettingsValidator().ValidateAndThrow(settings);

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
        }, Assembly.GetAssembly(typeof(ClassToReferenceAssembly)) ?? throw new InvalidOperationException());
        // Assembly.GetExecutingAssembly()

        services.AddHostedService<Worker>();

        AddCreationConsumer(services, settings);
        // AddNonCreationConsumer(services, settings);
    })
    .Build();

await host.RunAsync();
return;

void AddCreationConsumer(IServiceCollection serviceCollection, PulsarConsumerSettings pulsarConsumerSettings)
{
    serviceCollection.AddKeyedSingleton<IConsumer>("Consumer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IPulsarTopicCreationBuilder>()
        .WithWorkerName(pulsarConsumerSettings.WebAnalyticsTopicSettings.WorkerName)
        .WithTopicName(pulsarConsumerSettings.WebAnalyticsTopicSettings.Source)
        .WithConsumerGroup(pulsarConsumerSettings.WebAnalyticsTopicSettings.ConsumerGroup)
        .WithConfiguration(
            pulsarConsumerSettings.WebAnalyticsTopicSettings.Tenant,
            pulsarConsumerSettings.WebAnalyticsTopicSettings.Namespace,
            pulsarConsumerSettings.WebAnalyticsTopicSettings.NumberOfPartitions
        )
        .WithTopicOptions(pulsarConsumerSettings.WebAnalyticsTopicSettings.StartNewConsumerEarliest)
        .WithConsumeSettings(pulsarConsumerSettings.WebAnalyticsTopicSettings.NumberOfInstances)
        .BuildConsumerAsync(CancellationToken.None).Result);
}

void AddNonCreationConsumer(IServiceCollection serviceCollection, PulsarConsumerSettings pulsarConsumerSettings)
{
    serviceCollection.AddKeyedSingleton<IConsumer>("Consumer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IPulsarTopicBuilder>()
        .WithWorkerName(pulsarConsumerSettings.WebAnalyticsTopicSettings.WorkerName)
        .WithTopicName(pulsarConsumerSettings.WebAnalyticsTopicSettings.Source)
        .WithConsumerGroup(pulsarConsumerSettings.WebAnalyticsTopicSettings.ConsumerGroup)
        .WithConfiguration(
            pulsarConsumerSettings.WebAnalyticsTopicSettings.Tenant,
            pulsarConsumerSettings.WebAnalyticsTopicSettings.Namespace
        )
        .WithTopicOptions(pulsarConsumerSettings.WebAnalyticsTopicSettings.StartNewConsumerEarliest)
        .WithConsumeSettings(pulsarConsumerSettings.WebAnalyticsTopicSettings.NumberOfInstances)
        .BuildConsumerAsync(CancellationToken.None).Result);
}
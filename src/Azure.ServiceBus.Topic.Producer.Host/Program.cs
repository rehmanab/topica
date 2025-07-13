﻿using System.Reflection;
using Azure.ServiceBus.Topic.Producer.Host;
using Azure.ServiceBus.Topic.Producer.Host.Settings;
using Azure.ServiceBus.Topic.Producer.Host.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Azure.ServiceBus.Contracts;
using Topica.Contracts;

Console.WriteLine("******* Starting Azure.ServiceBus.Topic.Producer.Host *******");

var host = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration(builder =>
        {
            builder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .AddEnvironmentVariables();
#if DEBUG
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
#endif
        }
    )
    .ConfigureServices((ctx, services) =>
    {
        // Configuration
        var hostSettings = ctx.Configuration.GetSection(AzureServiceBusHostSettings.SectionName).Get<AzureServiceBusHostSettings>();
        var settings = ctx.Configuration.GetSection(AzureServiceBusProducerSettings.SectionName).Get<AzureServiceBusProducerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(AzureServiceBusHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (settings == null) throw new InvalidOperationException($"{nameof(AzureServiceBusProducerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new AzureServiceBusHostSettingsValidator().ValidateAndThrow(hostSettings);
        new AzureServiceBusProducerSettingsValidator().ValidateAndThrow(settings);

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
        services.AddAzureServiceBusTopica(c => { c.ConnectionString = hostSettings.ConnectionString; }, Assembly.GetExecutingAssembly());

        services.Configure<HostOptions>(options => { options.ShutdownTimeout = TimeSpan.FromSeconds(5); });

        services.AddHostedService<Worker>();

        AddCreationProducer(services, settings);
        // AddNonCreationProducer(services, settings);
    })
    .Build();

await host.RunAsync();
return;

void AddCreationProducer(IServiceCollection serviceCollection, AzureServiceBusProducerSettings azureServiceBusProducerSettings)
{
    serviceCollection.AddKeyedSingleton<IProducer>("Producer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IAzureServiceBusTopicCreationBuilder>()
        .WithWorkerName(azureServiceBusProducerSettings.WebAnalyticsTopicSettings.WorkerName)
        .WithTopicName(azureServiceBusProducerSettings.WebAnalyticsTopicSettings.Source)
        .WithSubscriptions(azureServiceBusProducerSettings.WebAnalyticsTopicSettings.Subscriptions)
        .WithSubscribeToSubscription(azureServiceBusProducerSettings.WebAnalyticsTopicSettings.SubscribeToSource)
        .WithTimings
        (
            azureServiceBusProducerSettings.WebAnalyticsTopicSettings.AutoDeleteOnIdle,
            azureServiceBusProducerSettings.WebAnalyticsTopicSettings.DefaultMessageTimeToLive,
            azureServiceBusProducerSettings.WebAnalyticsTopicSettings.DuplicateDetectionHistoryTimeWindow
        )
        .WithOptions
        (
            azureServiceBusProducerSettings.WebAnalyticsTopicSettings.EnableBatchedOperations,
            azureServiceBusProducerSettings.WebAnalyticsTopicSettings.EnablePartitioning,
            azureServiceBusProducerSettings.WebAnalyticsTopicSettings.MaxSizeInMegabytes,
            azureServiceBusProducerSettings.WebAnalyticsTopicSettings.RequiresDuplicateDetection,
            azureServiceBusProducerSettings.WebAnalyticsTopicSettings.MaxMessageSizeInKilobytes,
            azureServiceBusProducerSettings.WebAnalyticsTopicSettings.EnabledStatus,
            azureServiceBusProducerSettings.WebAnalyticsTopicSettings.SupportOrdering
        )
        .WithMetadata(azureServiceBusProducerSettings.WebAnalyticsTopicSettings.UserMetadata)
        .WithNumberOfInstances(1)
        .BuildProducerAsync(CancellationToken.None).Result);
}

void AddNonCreationProducer(IServiceCollection serviceCollection, AzureServiceBusProducerSettings azureServiceBusProducerSettings)
{
    serviceCollection.AddKeyedSingleton<IProducer>("Producer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IAzureServiceBusTopicBuilder>()
        .WithWorkerName(azureServiceBusProducerSettings.WebAnalyticsTopicSettings.WorkerName)
        .WithTopicName(azureServiceBusProducerSettings.WebAnalyticsTopicSettings.Source)
        .WithSubscribeToSubscription(azureServiceBusProducerSettings.WebAnalyticsTopicSettings.SubscribeToSource)
        .WithNumberOfInstances(1)
        .BuildProducerAsync(CancellationToken.None).Result);
}
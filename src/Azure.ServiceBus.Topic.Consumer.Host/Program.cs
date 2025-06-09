using System;
using System.IO;
using System.Reflection;
using Azure.ServiceBus.Topic.Consumer.Host;
using Azure.ServiceBus.Topic.Consumer.Host.Settings;
using Azure.ServiceBus.Topic.Consumer.Host.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Azure.ServiceBus.Contracts;
using Topica.SharedMessageHandlers;

Console.WriteLine("******* Starting AzureServiceBus.Consumer.Host *******");

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
    .ConfigureServices(services =>
    {
        services.AddLogging(configure => configure.AddSimpleConsole(x =>
        {
            x.IncludeScopes = false;
            x.TimestampFormat = "[HH:mm:ss] ";
            x.SingleLine = true;
        }));

        // Configuration
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        var hostSettings = configuration.GetSection(AzureServiceBusHostSettings.SectionName).Get<AzureServiceBusHostSettings>();
        var settings = configuration.GetSection(AzureServiceBusConsumerSettings.SectionName).Get<AzureServiceBusConsumerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(AzureServiceBusHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (settings == null) throw new InvalidOperationException($"{nameof(AzureServiceBusConsumerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new AzureServiceBusHostSettingsValidator().ValidateAndThrow(hostSettings);
        new AzureServiceBusConsumerSettingsValidator().ValidateAndThrow(settings);

        services.AddSingleton(hostSettings);
        services.AddSingleton(settings);

        // Add MessagingPlatform Components
        services.AddAzureServiceBusTopica(c => { c.ConnectionString = hostSettings.ConnectionString; }, Assembly.GetAssembly(typeof(ClassToReferenceAssembly)) ?? throw new InvalidOperationException());
        // Assembly.GetExecutingAssembly()

        services.AddHostedService<Worker>();

        // Creation Builder
        services.AddSingleton(services.BuildServiceProvider().GetRequiredService<IAzureServiceBusTopicCreationBuilder>()
            .WithWorkerName(settings.WebAnalyticsTopicSettings.WorkerName)
            .WithTopicName(settings.WebAnalyticsTopicSettings.Source)
            .WithSubscriptions(settings.WebAnalyticsTopicSettings.Subscriptions)
            .WithSubscribeToSubscription(settings.WebAnalyticsTopicSettings.SubscribeToSource)
            .WithTimings
            (
                settings.WebAnalyticsTopicSettings.AutoDeleteOnIdle,
                settings.WebAnalyticsTopicSettings.DefaultMessageTimeToLive,
                settings.WebAnalyticsTopicSettings.DuplicateDetectionHistoryTimeWindow
            )
            .WithOptions
            (
                settings.WebAnalyticsTopicSettings.EnableBatchedOperations,
                settings.WebAnalyticsTopicSettings.EnablePartitioning,
                settings.WebAnalyticsTopicSettings.MaxSizeInMegabytes,
                settings.WebAnalyticsTopicSettings.RequiresDuplicateDetection,
                settings.WebAnalyticsTopicSettings.MaxMessageSizeInKilobytes,
                settings.WebAnalyticsTopicSettings.EnabledStatus,
                settings.WebAnalyticsTopicSettings.SupportOrdering
            )
            .WithMetadata(settings.WebAnalyticsTopicSettings.UserMetadata));
    })
    .Build();

await host.RunAsync();
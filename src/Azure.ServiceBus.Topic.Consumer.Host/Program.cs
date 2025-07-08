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
using Topica.Contracts;
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
    .ConfigureServices((ctx, services) =>
    {
        // Configuration
        var hostSettings = ctx.Configuration.GetSection(AzureServiceBusHostSettings.SectionName).Get<AzureServiceBusHostSettings>();
        var settings = ctx.Configuration.GetSection(AzureServiceBusConsumerSettings.SectionName).Get<AzureServiceBusConsumerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(AzureServiceBusHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (settings == null) throw new InvalidOperationException($"{nameof(AzureServiceBusConsumerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new AzureServiceBusHostSettingsValidator().ValidateAndThrow(hostSettings);
        new AzureServiceBusConsumerSettingsValidator().ValidateAndThrow(settings);

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
        services.AddAzureServiceBusTopica(c => { c.ConnectionString = hostSettings.ConnectionString; }, Assembly.GetAssembly(typeof(ClassToReferenceAssembly)) ?? throw new InvalidOperationException());
        // Assembly.GetExecutingAssembly()

        services.AddHostedService<Worker>();

        AddCreationConsumer(services, settings);
        // AddNonCreationConsumer(services, settings);
    })
    .Build();

await host.RunAsync();
return;

void AddCreationConsumer(IServiceCollection serviceCollection, AzureServiceBusConsumerSettings azureServiceBusConsumerSettings)
{
    serviceCollection.AddKeyedSingleton<IConsumer>("Consumer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IAzureServiceBusTopicCreationBuilder>()
        .WithWorkerName(azureServiceBusConsumerSettings.WebAnalyticsTopicSettings.WorkerName)
        .WithTopicName(azureServiceBusConsumerSettings.WebAnalyticsTopicSettings.Source)
        .WithSubscriptions(azureServiceBusConsumerSettings.WebAnalyticsTopicSettings.Subscriptions)
        .WithSubscribeToSubscription(azureServiceBusConsumerSettings.WebAnalyticsTopicSettings.SubscribeToSource)
        .WithTimings
        (
            azureServiceBusConsumerSettings.WebAnalyticsTopicSettings.AutoDeleteOnIdle,
            azureServiceBusConsumerSettings.WebAnalyticsTopicSettings.DefaultMessageTimeToLive,
            azureServiceBusConsumerSettings.WebAnalyticsTopicSettings.DuplicateDetectionHistoryTimeWindow
        )
        .WithOptions
        (
            azureServiceBusConsumerSettings.WebAnalyticsTopicSettings.EnableBatchedOperations,
            azureServiceBusConsumerSettings.WebAnalyticsTopicSettings.EnablePartitioning,
            azureServiceBusConsumerSettings.WebAnalyticsTopicSettings.MaxSizeInMegabytes,
            azureServiceBusConsumerSettings.WebAnalyticsTopicSettings.RequiresDuplicateDetection,
            azureServiceBusConsumerSettings.WebAnalyticsTopicSettings.MaxMessageSizeInKilobytes,
            azureServiceBusConsumerSettings.WebAnalyticsTopicSettings.EnabledStatus,
            azureServiceBusConsumerSettings.WebAnalyticsTopicSettings.SupportOrdering
        )
        .WithMetadata(azureServiceBusConsumerSettings.WebAnalyticsTopicSettings.UserMetadata)
        .BuildConsumerAsync(CancellationToken.None).Result);
}

void AddNonCreationConsumer(IServiceCollection serviceCollection, AzureServiceBusConsumerSettings azureServiceBusConsumerSettings)
{
    serviceCollection.AddKeyedSingleton<IConsumer>("Consumer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IAzureServiceBusTopicBuilder>()
        .WithWorkerName(azureServiceBusConsumerSettings.WebAnalyticsTopicSettings.WorkerName)
        .WithTopicName(azureServiceBusConsumerSettings.WebAnalyticsTopicSettings.Source)
        .WithSubscribeToSubscription(azureServiceBusConsumerSettings.WebAnalyticsTopicSettings.SubscribeToSource)
        .BuildConsumerAsync(CancellationToken.None).Result);
}
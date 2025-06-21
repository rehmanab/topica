using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pulsar.Topic.Consumer.Host;
using Pulsar.Topic.Consumer.Host.Settings;
using Pulsar.Topic.Consumer.Host.Validators;
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
    .ConfigureServices(services =>
    {
        // Configuration
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        var hostSettings = configuration.GetSection(PulsarHostSettings.SectionName).Get<PulsarHostSettings>();
        var settings = configuration.GetSection(PulsarConsumerSettings.SectionName).Get<PulsarConsumerSettings>();

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
            .AddSeq(configuration.GetSection(SeqSettings.SectionName)));

        // Add MessagingPlatform Components
        services.AddPulsarTopica(c =>
        {
            c.ServiceUrl = hostSettings.ServiceUrl;
            c.PulsarManagerBaseUrl = hostSettings.PulsarManagerBaseUrl;
            c.PulsarAdminBaseUrl = hostSettings.PulsarAdminBaseUrl;
        }, Assembly.GetAssembly(typeof(ClassToReferenceAssembly)) ?? throw new InvalidOperationException());
        // Assembly.GetExecutingAssembly()

        services.AddHostedService<Worker>();

        // Creation Builder
        services.AddSingleton(services.BuildServiceProvider().GetRequiredService<IPulsarTopicCreationBuilder>()
            .WithWorkerName(settings.WebAnalyticsTopicSettings.WorkerName)
            .WithTopicName(settings.WebAnalyticsTopicSettings.Source)
            .WithConsumerGroup(settings.WebAnalyticsTopicSettings.ConsumerGroup)
            .WithConfiguration(
                settings.WebAnalyticsTopicSettings.Tenant,
                settings.WebAnalyticsTopicSettings.Namespace,
                settings.WebAnalyticsTopicSettings.NumberOfPartitions
            )
            .WithTopicOptions(settings.WebAnalyticsTopicSettings.StartNewConsumerEarliest)
            .WithConsumeSettings(settings.WebAnalyticsTopicSettings.NumberOfInstances));
    })
    .Build();

await host.RunAsync();
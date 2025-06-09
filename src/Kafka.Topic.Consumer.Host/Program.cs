using System.Reflection;
using FluentValidation;
using Kafka.Topic.Consumer.Host;
using Kafka.Topic.Consumer.Host.Settings;
using Kafka.Topic.Consumer.Host.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.SharedMessageHandlers;
using Topica.Kafka.Contracts;

Console.WriteLine("******* Starting Kafka.Topic.Consumer.Host *******");

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
        services.AddLogging(configure => configure.AddSimpleConsole(x =>
        {
            x.IncludeScopes = false;
            x.TimestampFormat = "[HH:mm:ss] ";
            x.SingleLine = true;
        }));

        // Configuration
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        var hostSettings = configuration.GetSection(KafkaHostSettings.SectionName).Get<KafkaHostSettings>();
        var settings = configuration.GetSection(KafkaConsumerSettings.SectionName).Get<KafkaConsumerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(KafkaHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (settings == null) throw new InvalidOperationException($"{nameof(KafkaConsumerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new KafkaHostSettingsValidator().ValidateAndThrow(hostSettings);
        new KafkaConsumerSettingsValidator().ValidateAndThrow(settings);

        services.AddSingleton(hostSettings);
        services.AddSingleton(settings);

        // Add MessagingPlatform Components
        services.AddKafkaTopica(Assembly.GetAssembly(typeof(ClassToReferenceAssembly)) ?? throw new InvalidOperationException());
        // Assembly.GetExecutingAssembly()

        services.Configure<HostOptions>(options => { options.ShutdownTimeout = TimeSpan.FromSeconds(5); });

        services.AddHostedService<Worker>();

        // Creation Builder
        services.AddSingleton(services.BuildServiceProvider().GetRequiredService<IKafkaTopicCreationBuilder>()
            .WithWorkerName(settings.WebAnalyticsTopicSettings.WorkerName)
            .WithTopicName(settings.WebAnalyticsTopicSettings.Source)
            .WithConsumerGroup(settings.WebAnalyticsTopicSettings.ConsumerGroup)
            .WithTopicSettings(settings.WebAnalyticsTopicSettings.StartFromEarliestMessages, settings.WebAnalyticsTopicSettings.NumberOfTopicPartitions)
            .WithBootstrapServers(hostSettings.BootstrapServers)
            .WithConsumeSettings(settings.WebAnalyticsTopicSettings.NumberOfInstances));
    })
    .Build();
await host.RunAsync();
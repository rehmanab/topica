using System.Reflection;
using FluentValidation;
using Kafka.Topic.Consumer.Host;
using Kafka.Topic.Consumer.Host.Settings;
using Kafka.Topic.Consumer.Host.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
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
    .ConfigureServices((ctx, services) =>
    {
        // Configuration
        var hostSettings = ctx.Configuration.GetSection(KafkaHostSettings.SectionName).Get<KafkaHostSettings>();
        var settings = ctx.Configuration.GetSection(KafkaConsumerSettings.SectionName).Get<KafkaConsumerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(KafkaHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (settings == null) throw new InvalidOperationException($"{nameof(KafkaConsumerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new KafkaHostSettingsValidator().ValidateAndThrow(hostSettings);
        new KafkaConsumerSettingsValidator().ValidateAndThrow(settings);

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
        services.AddKafkaTopica(c => { c.BootstrapServers = hostSettings.BootstrapServers; }, Assembly.GetAssembly(typeof(ClassToReferenceAssembly)) ?? throw new InvalidOperationException());
        // Assembly.GetExecutingAssembly()

        services.Configure<HostOptions>(options => { options.ShutdownTimeout = TimeSpan.FromSeconds(5); });

        services.AddHostedService<Worker>();

        AddCreationConsumer(services, settings, hostSettings);
        // AddNonCreationConsumer(services, settings, hostSettings);
    })
    .Build();

await host.RunAsync();
return;

void AddCreationConsumer(IServiceCollection serviceCollection, KafkaConsumerSettings kafkaConsumerSettings, KafkaHostSettings kafkaHostSettings)
{
    serviceCollection.AddKeyedSingleton<IConsumer>("Consumer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IKafkaTopicCreationBuilder>()
        .WithWorkerName(kafkaConsumerSettings.WebAnalyticsTopicSettings.WorkerName)
        .WithTopicName(kafkaConsumerSettings.WebAnalyticsTopicSettings.Source)
        .WithConsumerGroup(kafkaConsumerSettings.WebAnalyticsTopicSettings.ConsumerGroup)
        .WithTopicSettings(kafkaConsumerSettings.WebAnalyticsTopicSettings.StartFromEarliestMessages, kafkaConsumerSettings.WebAnalyticsTopicSettings.NumberOfTopicPartitions)
        .WithBootstrapServers(kafkaHostSettings.BootstrapServers)
        .WithConsumeSettings(kafkaConsumerSettings.WebAnalyticsTopicSettings.NumberOfInstances)
        .BuildConsumerAsync(CancellationToken.None).Result);
}

void AddNonCreationConsumer(IServiceCollection serviceCollection, KafkaConsumerSettings kafkaConsumerSettings, KafkaHostSettings kafkaHostSettings)
{
    serviceCollection.AddKeyedSingleton<IConsumer>("Consumer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IKafkaTopicBuilder>()
        .WithWorkerName(kafkaConsumerSettings.WebAnalyticsTopicSettings.WorkerName)
        .WithTopicName(kafkaConsumerSettings.WebAnalyticsTopicSettings.Source)
        .WithConsumerGroup(kafkaConsumerSettings.WebAnalyticsTopicSettings.ConsumerGroup)
        .WithTopicSettings(kafkaConsumerSettings.WebAnalyticsTopicSettings.StartFromEarliestMessages)
        .WithBootstrapServers(kafkaHostSettings.BootstrapServers)
        .WithConsumeSettings(kafkaConsumerSettings.WebAnalyticsTopicSettings.NumberOfInstances)
        .BuildConsumerAsync(CancellationToken.None).Result);
}
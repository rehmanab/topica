using System.Reflection;
using FluentValidation;
using Kafka.Topic.Producer.Host;
using Kafka.Topic.Producer.Host.Settings;
using Kafka.Topic.Producer.Host.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Kafka.Contracts;

Console.WriteLine("******* Starting Kafka.Topic.Producer.Host *******");

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
        var settings = ctx.Configuration.GetSection(KafkaProducerSettings.SectionName).Get<KafkaProducerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(KafkaHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (settings == null) throw new InvalidOperationException($"{nameof(KafkaProducerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new KafkaHostSettingsValidator().ValidateAndThrow(hostSettings);
        new KafkaProducerSettingsValidator().ValidateAndThrow(settings);

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
        services.AddKafkaTopica(c => { c.BootstrapServers = hostSettings.BootstrapServers; }, Assembly.GetExecutingAssembly());

        services.Configure<HostOptions>(options => { options.ShutdownTimeout = TimeSpan.FromSeconds(5); });

        services.AddHostedService<Worker>();

        AddCreationProducer(services, settings, hostSettings);
        // AddNonCreationProducer(services, settings, hostSettings);
    })
    .Build();

await host.RunAsync();
return;

void AddCreationProducer(IServiceCollection serviceCollection, KafkaProducerSettings kafkaProducerSettings, KafkaHostSettings kafkaHostSettings)
{
    serviceCollection.AddKeyedSingleton<IProducer>("Producer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IKafkaTopicCreationBuilder>()
        .WithWorkerName(kafkaProducerSettings.WebAnalyticsTopicSettings.WorkerName)
        .WithTopicName(kafkaProducerSettings.WebAnalyticsTopicSettings.Source)
        .WithConsumerGroup(kafkaProducerSettings.WebAnalyticsTopicSettings.ConsumerGroup)
        .WithTopicSettings(kafkaProducerSettings.WebAnalyticsTopicSettings.StartFromEarliestMessages, kafkaProducerSettings.WebAnalyticsTopicSettings.NumberOfTopicPartitions)
        .WithBootstrapServers(kafkaHostSettings.BootstrapServers)
        .BuildProducerAsync(CancellationToken.None).Result);
}

void AddNonCreationProducer(IServiceCollection serviceCollection, KafkaProducerSettings kafkaProducerSettings, KafkaHostSettings kafkaHostSettings)
{
    serviceCollection.AddKeyedSingleton<IProducer>("Producer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IKafkaTopicBuilder>()
        .WithWorkerName(kafkaProducerSettings.WebAnalyticsTopicSettings.WorkerName)
        .WithTopicName(kafkaProducerSettings.WebAnalyticsTopicSettings.Source)
        .WithConsumerGroup(kafkaProducerSettings.WebAnalyticsTopicSettings.ConsumerGroup)
        .WithTopicSettings(kafkaProducerSettings.WebAnalyticsTopicSettings.StartFromEarliestMessages)
        .WithBootstrapServers(kafkaHostSettings.BootstrapServers)
        .BuildProducerAsync(CancellationToken.None).Result);
}
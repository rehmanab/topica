using System.Reflection;
using Aws.Queue.Consumer.Host;
using Aws.Queue.Consumer.Host.Settings;
using Aws.Queue.Consumer.Host.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Aws.Contracts;
using Topica.SharedMessageHandlers;

Console.WriteLine("******* Starting Aws.Queue.Consumer.Host *******");

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
        var hostSettings = configuration.GetSection(AwsHostSettings.SectionName).Get<AwsHostSettings>();
        var settings = configuration.GetSection(AwsConsumerSettings.SectionName).Get<AwsConsumerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(AwsHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (settings == null) throw new InvalidOperationException($"{nameof(AwsConsumerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new AwsHostSettingsValidator().ValidateAndThrow(hostSettings);
        new AwsConsumerSettingsValidator().ValidateAndThrow(settings);

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
        services.AddAwsTopica(c =>
        {
            c.ProfileName = hostSettings.ProfileName;
            c.AccessKey = hostSettings.AccessKey;
            c.SecretKey = hostSettings.SecretKey;
            c.ServiceUrl = hostSettings.ServiceUrl;
            c.RegionEndpoint = hostSettings.RegionEndpoint;
        }, Assembly.GetAssembly(typeof(ClassToReferenceAssembly)) ?? throw new InvalidOperationException());
        // Assembly.GetExecutingAssembly()

        services.AddHostedService<Worker>();

        // Creation Builder
        services.AddSingleton(services.BuildServiceProvider().GetRequiredService<IAwsQueueCreationBuilder>()
            .WithWorkerName(settings.WebAnalyticsQueueSettings.WorkerName)
            .WithQueueName(settings.WebAnalyticsQueueSettings.Source)
            .WithErrorQueueSettings(
                settings.WebAnalyticsQueueSettings.BuildWithErrorQueue,
                settings.WebAnalyticsQueueSettings.ErrorQueueMaxReceiveCount
            )
            .WithFifoSettings(
                settings.WebAnalyticsQueueSettings.IsFifoQueue,
                settings.WebAnalyticsQueueSettings.IsFifoContentBasedDeduplication
            )
            .WithTemporalSettings(
                settings.WebAnalyticsQueueSettings.MessageVisibilityTimeoutSeconds,
                settings.WebAnalyticsQueueSettings.QueueMessageDelaySeconds,
                settings.WebAnalyticsQueueSettings.QueueMessageRetentionPeriodSeconds,
                settings.WebAnalyticsQueueSettings.QueueReceiveMessageWaitTimeSeconds
            )
            .WithQueueSettings(settings.WebAnalyticsQueueSettings.QueueMaximumMessageSize)
            .WithConsumeSettings(
                settings.WebAnalyticsQueueSettings.NumberOfInstances,
                settings.WebAnalyticsQueueSettings.QueueReceiveMaximumNumberOfMessages
            ));
    })
    .Build();

await host.RunAsync();
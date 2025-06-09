using System.Reflection;
using Aws.Queue.Producer.Host;
using Aws.Queue.Producer.Host.Settings;
using Aws.Queue.Producer.Host.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Aws.Contracts;

Console.WriteLine("******* Starting Aws.Queue.Producer.Host *******");

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
        var hostSettings = configuration.GetSection(AwsHostSettings.SectionName).Get<AwsHostSettings>();
        var settings = configuration.GetSection(AwsProducerSettings.SectionName).Get<AwsProducerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(AwsHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (settings == null) throw new InvalidOperationException($"{nameof(AwsProducerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new AwsHostSettingsValidator().ValidateAndThrow(hostSettings);
        new AwsProducerSettingsValidator().ValidateAndThrow(settings);

        services.AddSingleton(hostSettings);
        services.AddSingleton(settings);

        // Add MessagingPlatform Components
        services.AddAwsTopica(c =>
        {
            c.ProfileName = hostSettings.ProfileName;
            c.AccessKey = hostSettings.AccessKey;
            c.SecretKey = hostSettings.SecretKey;
            c.ServiceUrl = hostSettings.ServiceUrl;
            c.RegionEndpoint = hostSettings.RegionEndpoint;
        }, Assembly.GetExecutingAssembly());

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
                null, 
                settings.WebAnalyticsQueueSettings.QueueReceiveMaximumNumberOfMessages
            ));
    })
    .Build();

await host.RunAsync();
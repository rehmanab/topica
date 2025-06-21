using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMq.Queue.Consumer.Host;
using RabbitMq.Queue.Consumer.Host.Settings;
using RabbitMq.Queue.Consumer.Host.Validators;
using Topica.SharedMessageHandlers;
using Topica.RabbitMq.Contracts;

Console.WriteLine("******* Starting RabbitMq.Queue.Consumer.Host *******");

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
        var hostSettings = configuration.GetSection(RabbitMqHostSettings.SectionName).Get<RabbitMqHostSettings>();
        var settings = configuration.GetSection(RabbitMqConsumerSettings.SectionName).Get<RabbitMqConsumerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(RabbitMqHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (settings == null) throw new InvalidOperationException($"{nameof(RabbitMqConsumerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new RabbitMqHostSettingsValidator().ValidateAndThrow(hostSettings);
        new RabbitMqConsumerSettingsValidator().ValidateAndThrow(settings);

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
        services.AddRabbitMqTopica(c =>
        {
            c.Hostname = hostSettings.Hostname;
            c.UserName = hostSettings.UserName;
            c.Password = hostSettings.Password;
            c.Scheme = hostSettings.Scheme;
            c.Port = hostSettings.Port;
            c.ManagementPort = hostSettings.ManagementPort;
            c.ManagementScheme = hostSettings.ManagementScheme;
            c.VHost = hostSettings.VHost;
        }, Assembly.GetAssembly(typeof(ClassToReferenceAssembly)) ?? throw new InvalidOperationException());
        // Assembly.GetExecutingAssembly()

        services.AddHostedService<Worker>();

        // Creation Builder
        services.AddSingleton(services.BuildServiceProvider().GetRequiredService<IRabbitMqQueueCreationBuilder>()
            .WithWorkerName(settings.WebAnalyticsTopicSettings.WorkerName)
            .WithQueueName(settings.WebAnalyticsTopicSettings.Source)
            .WithConsumeSettings(settings.WebAnalyticsTopicSettings.NumberOfInstances));
    })
    .Build();

await host.RunAsync();
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMq.Queue.Consumer.Host;
using RabbitMq.Queue.Consumer.Host.Settings;
using RabbitMq.Queue.Consumer.Host.Validators;
using Topica.Contracts;
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
    .ConfigureServices((ctx, services) =>
    {
        // Configuration
        var hostSettings = ctx.Configuration.GetSection(RabbitMqHostSettings.SectionName).Get<RabbitMqHostSettings>();
        var settings = ctx.Configuration.GetSection(RabbitMqConsumerSettings.SectionName).Get<RabbitMqConsumerSettings>();

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
            .AddSeq(ctx.Configuration.GetSection(SeqSettings.SectionName)));

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

        AddCreationConsumer(services, settings);
        // AddNonCreationConsumer(services, settings);
    })
    .Build();

await host.RunAsync();
return;

void AddCreationConsumer(IServiceCollection serviceCollection, RabbitMqConsumerSettings rabbitMqConsumerSettings)
{
    serviceCollection.AddKeyedSingleton<IConsumer>("Consumer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IRabbitMqQueueCreationBuilder>()
        .WithWorkerName(rabbitMqConsumerSettings.WebAnalyticsQueueSettings.WorkerName)
        .WithQueueName(rabbitMqConsumerSettings.WebAnalyticsQueueSettings.Source)
        .WithConsumeSettings(rabbitMqConsumerSettings.WebAnalyticsQueueSettings.NumberOfInstances)
        .BuildConsumerAsync(CancellationToken.None).Result);
}

void AddNonCreationConsumer(IServiceCollection serviceCollection, RabbitMqConsumerSettings rabbitMqConsumerSettings)
{
    serviceCollection.AddKeyedSingleton<IConsumer>("Consumer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IRabbitMqQueueBuilder>()
        .WithWorkerName(rabbitMqConsumerSettings.WebAnalyticsQueueSettings.WorkerName)
        .WithQueueName(rabbitMqConsumerSettings.WebAnalyticsQueueSettings.Source)
        .WithConsumeSettings(rabbitMqConsumerSettings.WebAnalyticsQueueSettings.NumberOfInstances)
        .BuildConsumerAsync(CancellationToken.None).Result);
}
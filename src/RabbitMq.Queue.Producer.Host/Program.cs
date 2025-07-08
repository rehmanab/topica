using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMq.Queue.Producer.Host;
using RabbitMq.Queue.Producer.Host.Settings;
using RabbitMq.Queue.Producer.Host.Validators;
using Topica.Contracts;
using Topica.RabbitMq.Contracts;

Console.WriteLine("******* Starting RabbitMq.Queue.Producer.Host *******");

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
        var settings = ctx.Configuration.GetSection(RabbitMqProducerSettings.SectionName).Get<RabbitMqProducerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(RabbitMqHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (settings == null) throw new InvalidOperationException($"{nameof(RabbitMqProducerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new RabbitMqHostSettingsValidator().ValidateAndThrow(hostSettings);
        new RabbitMqProducerSettingsValidator().ValidateAndThrow(settings);

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
        }, Assembly.GetExecutingAssembly());

        services.Configure<HostOptions>(options => { options.ShutdownTimeout = TimeSpan.FromSeconds(5); });

        services.AddHostedService<Worker>();

        AddCreationProducer(services, settings);
        // AddNonCreationProducer(services, settings);
    })
    .Build();

await host.RunAsync();
return;

void AddCreationProducer(IServiceCollection serviceCollection, RabbitMqProducerSettings rabbitMqProducerSettings)
{
    serviceCollection.AddKeyedSingleton<IProducer>("Producer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IRabbitMqQueueCreationBuilder>()
        .WithWorkerName(rabbitMqProducerSettings.WebAnalyticsTopicSettings.WorkerName)
        .WithQueueName(rabbitMqProducerSettings.WebAnalyticsTopicSettings.Source)
        .BuildProducerAsync(CancellationToken.None).Result);
}

void AddNonCreationProducer(IServiceCollection serviceCollection, RabbitMqProducerSettings rabbitMqProducerSettings)
{
    serviceCollection.AddKeyedSingleton<IProducer>("Producer", (_, _) => serviceCollection.BuildServiceProvider().GetRequiredService<IRabbitMqQueueBuilder>()
        .WithWorkerName(rabbitMqProducerSettings.WebAnalyticsTopicSettings.WorkerName)
        .WithQueueName(rabbitMqProducerSettings.WebAnalyticsTopicSettings.Source)
        .BuildProducerAsync(CancellationToken.None).Result);
}
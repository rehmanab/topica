using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMq.Queue.Consumer.Host;
using RabbitMq.Queue.Consumer.Host.Settings;
using RabbitMq.Queue.Consumer.Host.Validators;
using Topica.Host.Shared;

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
        services.AddLogging(configure => configure.AddSimpleConsole(x =>
        {
            x.IncludeScopes = false;
            x.TimestampFormat = "[HH:mm:ss] ";
            x.SingleLine = true;
        }));

        // Configuration
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        var hostSettings = configuration.GetSection(RabbitMqHostSettings.SectionName).Get<RabbitMqHostSettings>();
        var consumerSettings = configuration.GetSection(RabbitMqConsumerSettings.SectionName).Get<RabbitMqConsumerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(RabbitMqHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (consumerSettings == null) throw new InvalidOperationException($"{nameof(RabbitMqConsumerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new RabbitMqHostSettingsValidator().ValidateAndThrow(hostSettings);
        new RabbitMqConsumerSettingsValidator().ValidateAndThrow(consumerSettings);

        services.AddSingleton(hostSettings);
        services.AddSingleton(consumerSettings);

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
    })
    .Build();

await host.RunAsync();
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMq.Topic.Producer.Host;
using RabbitMq.Topic.Producer.Host.Settings;
using RabbitMq.Topic.Producer.Host.Validators;
using Topica.Settings;

Console.WriteLine("******* Starting RabbitMq.Topic.Producer.Host *******");

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
        var consumerSettings = configuration.GetSection(RabbitMqProducerSettings.SectionName).Get<RabbitMqProducerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(RabbitMqHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (consumerSettings == null) throw new InvalidOperationException($"{nameof(RabbitMqProducerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new RabbitMqHostSettingsValidator().ValidateAndThrow(hostSettings);
        new RabbitMqProducerSettingsValidator().ValidateAndThrow(consumerSettings);
        
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
        }, Assembly.GetExecutingAssembly());
        
        services.Configure<HostOptions>(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(5);
        });
        
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
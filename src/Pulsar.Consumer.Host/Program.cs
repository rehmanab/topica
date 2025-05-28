using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pulsar.Consumer.Host;
using Pulsar.Consumer.Host.Settings;
using Pulsar.Consumer.Host.Validators;

Console.WriteLine("******* Starting Pulsar.Consumer.Host *******");

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
        var hostSettings = configuration.GetSection(PulsarHostSettings.SectionName).Get<PulsarHostSettings>();
        var consumerSettings = configuration.GetSection(PulsarConsumerSettings.SectionName).Get<PulsarConsumerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(PulsarHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (consumerSettings == null) throw new InvalidOperationException($"{nameof(PulsarConsumerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new PulsarHostSettingsValidator().ValidateAndThrow(hostSettings);
        new PulsarConsumerSettingsValidator().ValidateAndThrow(consumerSettings);
        
        services.AddSingleton(hostSettings);
        services.AddSingleton(consumerSettings);
        
        // Add MessagingPlatform Components
        services.AddPulsarTopica(c =>
        {
            c.ServiceUrl = hostSettings.ServiceUrl;
            c.PulsarManagerBaseUrl = hostSettings.PulsarManagerBaseUrl;
            c.PulsarAdminBaseUrl = hostSettings.PulsarAdminBaseUrl;
        }, Assembly.GetExecutingAssembly());
        
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
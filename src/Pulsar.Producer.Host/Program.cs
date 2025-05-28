using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pulsar.Producer.Host;
using Pulsar.Producer.Host.Settings;
using Pulsar.Producer.Host.Validators;
using Topica.Settings;

Console.WriteLine("******* Starting Pulsar.Producer.Host *******");

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
        var producerSettings = configuration.GetSection(PulsarProducerSettings.SectionName).Get<PulsarProducerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(PulsarHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (producerSettings == null) throw new InvalidOperationException($"{nameof(PulsarProducerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new PulsarHostSettingsValidator().ValidateAndThrow(hostSettings);
        new PulsarProducerSettingsValidator().ValidateAndThrow(producerSettings);
        
        services.AddSingleton(hostSettings);
        services.AddSingleton(producerSettings);

        // Add MessagingPlatform Components
        services.AddPulsarTopica(c =>
        {
            c.ServiceUrl = hostSettings.ServiceUrl;
            c.PulsarManagerBaseUrl = hostSettings.PulsarManagerBaseUrl;
            c.PulsarAdminBaseUrl = hostSettings.PulsarAdminBaseUrl;
        }, Assembly.GetExecutingAssembly());
        
        services.Configure<HostOptions>(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(5);
        });
        
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
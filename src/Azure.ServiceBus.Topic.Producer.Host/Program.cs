using System.Reflection;
using Azure.ServiceBus.Topic.Producer.Host;
using Azure.ServiceBus.Topic.Producer.Host.Settings;
using Azure.ServiceBus.Topic.Producer.Host.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Settings;

Console.WriteLine("******* Starting Azure.ServiceBus.Topic.Producer.Host *******");

var host = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration(builder =>
        {
            builder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .AddEnvironmentVariables();
#if DEBUG
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
#endif
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
        var hostSettings = configuration.GetSection(AzureServiceBusHostSettings.SectionName).Get<AzureServiceBusHostSettings>();
        var producerSettings = configuration.GetSection(AzureServiceBusProducerSettings.SectionName).Get<AzureServiceBusProducerSettings>();
        
        if (hostSettings == null) throw new InvalidOperationException($"{nameof(AzureServiceBusHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (producerSettings == null) throw new InvalidOperationException($"{nameof(AzureServiceBusProducerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new AzureServiceBusHostSettingsValidator().ValidateAndThrow(hostSettings);
        new AzureServiceBusProducerSettingsValidator().ValidateAndThrow(producerSettings);

        services.AddSingleton(hostSettings);
        services.AddSingleton(producerSettings);

        // Add MessagingPlatform Components
        services.AddAzureServiceBusTopica(c => { c.ConnectionString = hostSettings.ConnectionString; }, Assembly.GetExecutingAssembly());

        services.Configure<HostOptions>(options => { options.ShutdownTimeout = TimeSpan.FromSeconds(5); });

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
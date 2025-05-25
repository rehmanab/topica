using System;
using System.IO;
using System.Reflection;
using Azure.ServiceBus.Consumer.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Azure.ServiceBus.Settings;

Console.WriteLine("******* Starting AzureServiceBus.Consumer.Host *******");

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
        var hostSettings = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        var azureServiceBusHostSettings = hostSettings.GetSection(AzureServiceBusHostSettings.SectionName).Get<AzureServiceBusHostSettings>();
        if (azureServiceBusHostSettings == null || string.IsNullOrEmpty(azureServiceBusHostSettings.ConnectionString))
        {
            throw new ApplicationException("AzureServiceBusHostSettings not found or ConnectionString is empty");
        }
        services.AddSingleton(azureServiceBusHostSettings);
        
        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            return config.GetSection(AzureServiceBusConsumerSettings.SectionName).Get<AzureServiceBusConsumerSettings>() ?? throw new ApplicationException("AzureServiceBusConsumerSettings not found");
        });
        
        // Add MessagingPlatform Components
        services.AddAzureServiceBusTopica(c =>
        {
            c.ConnectionString = azureServiceBusHostSettings.ConnectionString;
        }, Assembly.GetExecutingAssembly());
        
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
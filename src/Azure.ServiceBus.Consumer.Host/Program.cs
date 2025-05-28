using System;
using System.IO;
using System.Reflection;
using Azure.ServiceBus.Consumer.Host;
using Azure.ServiceBus.Consumer.Host.Settings;
using Azure.ServiceBus.Consumer.Host.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        var hostSettings = configuration.GetSection(AzureServiceBusHostSettings.SectionName).Get<AzureServiceBusHostSettings>();
        var consumerSettings = configuration.GetSection(AzureServiceBusConsumerSettings.SectionName).Get<AzureServiceBusConsumerSettings>();
        
        if (hostSettings == null) throw new InvalidOperationException($"{nameof(AzureServiceBusHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (consumerSettings == null) throw new InvalidOperationException($"{nameof(AzureServiceBusConsumerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new AzureServiceBusHostSettingsValidator().ValidateAndThrow(hostSettings);
        new AzureServiceBusConsumerSettingsValidator().ValidateAndThrow(consumerSettings);
        
        services.AddSingleton(hostSettings);
        services.AddSingleton(consumerSettings);
        
        // Add MessagingPlatform Components
        services.AddAzureServiceBusTopica(c =>
        {
            c.ConnectionString = hostSettings.ConnectionString;
        }, Assembly.GetExecutingAssembly());
        
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
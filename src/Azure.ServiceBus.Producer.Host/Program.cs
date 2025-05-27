using System.Reflection;
using Azure.ServiceBus.Producer.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Azure.ServiceBus.Settings;
using Topica.Settings;

Console.WriteLine("******* Starting Azure.ServiceBus.Producer.Host *******");

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
            return config.GetSection(ProducerSettings.SectionName).Get<ProducerSettings>() ?? throw new InvalidOperationException("ConsumerSettings not found");
        });

        // Add MessagingPlatform Components
        services.AddAzureServiceBusTopica(c => { c.ConnectionString = azureServiceBusHostSettings.ConnectionString; }, Assembly.GetExecutingAssembly());

        services.Configure<HostOptions>(options => { options.ShutdownTimeout = TimeSpan.FromSeconds(5); });

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
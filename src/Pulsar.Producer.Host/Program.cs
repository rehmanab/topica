using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pulsar.Producer.Host;
using Topica.Pulsar.Settings;
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
        var hostSettings = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        var pulsarHostSettings = hostSettings.GetSection(PulsarHostSettings.SectionName).Get<PulsarHostSettings>();

        if (pulsarHostSettings == null)
        {
            throw new ApplicationException("PulsarHostSettings not found");
        }
        
        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            return config.GetSection(ProducerSettings.SectionName).Get<ProducerSettings>() ?? throw new InvalidOperationException("ConsumerSettings not found");
        });

        // Add MessagingPlatform Components
        services.AddPulsarTopica(c =>
        {
            c.ServiceUrl = pulsarHostSettings.ServiceUrl;
            c.PulsarManagerBaseUrl = pulsarHostSettings.PulsarManagerBaseUrl;
            c.PulsarAdminBaseUrl = pulsarHostSettings.PulsarAdminBaseUrl;
        }, Assembly.GetExecutingAssembly());
        
        services.Configure<HostOptions>(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(5);
        });
        
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
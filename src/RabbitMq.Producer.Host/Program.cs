using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMq.Producer.Host;
using Topica.RabbitMq.Settings;
using Topica.Settings;

Console.WriteLine("******* Starting RabbitMq.Producer.Host *******");

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
        var rabbitMqHostSettings = hostSettings.GetSection(RabbitMqHostSettings.SectionName).Get<RabbitMqHostSettings>() ?? throw new InvalidOperationException("RabbitMqHostSettings not found");

        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            return config.GetSection(ProducerSettings.SectionName).Get<ProducerSettings>() ?? throw new InvalidOperationException("ConsumerSettings not found");
        });

        // Add MessagingPlatform Components
        services.AddRabbitMqTopica(c =>
        {
            c.Hostname = rabbitMqHostSettings.Hostname;
            c.UserName = rabbitMqHostSettings.UserName;
            c.Password = rabbitMqHostSettings.Password;
            c.Scheme = rabbitMqHostSettings.Scheme;
            c.Port = rabbitMqHostSettings.Port;
            c.ManagementPort = rabbitMqHostSettings.ManagementPort;
            c.ManagementScheme = rabbitMqHostSettings.ManagementScheme;
            c.VHost = rabbitMqHostSettings.VHost;
        }, Assembly.GetExecutingAssembly());
        
        services.Configure<HostOptions>(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(5);
        });
        
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
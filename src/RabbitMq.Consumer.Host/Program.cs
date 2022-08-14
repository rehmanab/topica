using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMq.Consumer.Host;
using RabbitMq.Consumer.Host.Settings;
using Topica.RabbitMq.Settings;

Console.WriteLine("******* Starting RabbitMq.Consumer.Host *******");

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
        // Configuration
        var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        var rabbitMqSettings = config.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>();
        services.AddSingleton(provider => provider.GetRequiredService<IConfiguration>().GetSection(ConsumerSettings.SectionName).Get<ConsumerSettings>());
        
        // Add MessagingPlatform Components
        services.AddRabbitMqTopica(c =>
        {
            c.Hostname = rabbitMqSettings.Hostname;
            c.UserName = rabbitMqSettings.UserName;
            c.Password = rabbitMqSettings.Password;
            c.Scheme = rabbitMqSettings.Scheme;
            c.Port = rabbitMqSettings.Port;
            c.ManagementPort = rabbitMqSettings.ManagementPort;
            c.ManagementScheme = rabbitMqSettings.ManagementScheme;
            c.VHost = rabbitMqSettings.VHost;
        });
        
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
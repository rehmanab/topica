using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMq.Consumer.Host;
using RabbitMq.Consumer.Host.Settings;
using Topica.Settings;

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
        var hostSettings = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        var rabbitMqHostSettings = hostSettings.GetSection(RabbitMqHostSettings.SectionName).Get<RabbitMqHostSettings>();
        
        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            return config.GetSection(ConsumerSettings.SectionName).Get<IEnumerable<ConsumerSettings>>();
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
        });
        
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
using System.Reflection;
using FluentValidation;
using Kafka.Topic.Producer.Host;
using Kafka.Topic.Producer.Host.Settings;
using Kafka.Topic.Producer.Host.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Settings;

Console.WriteLine("******* Starting Kafka.Topic.Producer.Host *******");

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
        var hostSettings = configuration.GetSection(KafkaHostSettings.SectionName).Get<KafkaHostSettings>();
        var consumerSettings = configuration.GetSection(KafkaProducerSettings.SectionName).Get<KafkaProducerSettings>();

        if (hostSettings == null) throw new InvalidOperationException($"{nameof(KafkaHostSettings)} is not configured. Please check your appsettings.json or environment variables.");
        if (consumerSettings == null) throw new InvalidOperationException($"{nameof(KafkaProducerSettings)} is not configured. Please check your appsettings.json or environment variables.");

        new KafkaHostSettingsValidator().ValidateAndThrow(hostSettings);
        new KafkaProducerSettingsValidator().ValidateAndThrow(consumerSettings);
        
        services.AddSingleton(hostSettings);
        services.AddSingleton(consumerSettings);

        // Add MessagingPlatform Components
        services.AddKafkaTopica(Assembly.GetExecutingAssembly());

        services.Configure<HostOptions>(options => { options.ShutdownTimeout = TimeSpan.FromSeconds(5); });

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
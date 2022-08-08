using System.Reflection;
using Kafka.Consumer.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Topica.Contracts;
using Topica.Executors;
using Topica.Kafka.Settings;
using Topica.Resolvers;

Console.WriteLine("******* Starting Kafka.Consumer.Host *******");

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
        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            return config.GetSection(ConsumerSettings.SectionName).Get<ConsumerSettings>();
        });
        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            return config.GetSection(KafkaSettings.SectionName).Get<KafkaSettings>();
        });
        
        // Add MessagingPlatform Components
        services.AddKafkaTopica();
        services.AddHostedService<Worker>();
        
        // Handlers
        services.AddTransient<IHandlerResolver>(_ => new HandlerResolver(services.BuildServiceProvider(), Assembly.GetExecutingAssembly()));
        services.AddTransient<IMessageHandlerExecutor, MessageHandlerExecutor>();
            
        services.Scan(s => s
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(c => c.AssignableTo(typeof(IHandler<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());
    })
    .Build();

await host.RunAsync();
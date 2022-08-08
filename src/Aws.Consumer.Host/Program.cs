// See https://aka.ms/new-console-template for more information

using System.Reflection;
using Aws.Consumer.Host;
using Aws.Consumer.Host.Handlers;
using Aws.Consumer.Host.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Topica.Contracts;
using Topica.Executors;
using Topica.Resolvers;

Console.WriteLine("******* Starting Aws.Consumer.Host *******");

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
        
        services.AddAwsTopica("http://dockerhost:4566");
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
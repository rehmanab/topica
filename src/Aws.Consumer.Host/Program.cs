// See https://aka.ms/new-console-template for more information

using Aws.Consumer.Host;
using Aws.Consumer.Host.Handlers;
using Aws.Consumer.Host.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Topica.Contracts;

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
        services.AddScoped<IHandler<OrderMessage>, OrderHandler>();
        services.AddScoped<IHandler<CustomerMessage>, CustomerHandler>();
    })
    .Build();

await host.RunAsync();
// See https://aka.ms/new-console-template for more information

using Aws.Consumer.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        services.AddAwsTopica("http://dockerhost:4566");
        services.AddSingleton(new WorkerConfiguration
        {
            QueueName = "ar-sqs-test-1_1.fifo", 
            DelayMilliseconds = 2000, 
            NumberOfThreads = 10
        });
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
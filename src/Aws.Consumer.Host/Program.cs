// See https://aka.ms/new-console-template for more information

using Aws.Consumer.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("******* Starting Aws.Consumer.Host *******");

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddAwsTopica("http://dockerhost:4566");
        services.AddSingleton<IWorkerConfiguration>(new WorkerConfiguration { DelayMilliseconds = 2000 });
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();

public interface IWorkerConfiguration
{
    int DelayMilliseconds { get; set; }
}

public class WorkerConfiguration : IWorkerConfiguration
{
    public int DelayMilliseconds { get; set; }
}
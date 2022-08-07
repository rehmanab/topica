using System.Reflection;
using Aws.Consumer.Host.Handlers;
using Microsoft.Extensions.Hosting;
using Topica.Aws.Queues;

namespace Aws.Consumer.Host;

public class Worker : BackgroundService
{
    private readonly IAwsQueueConsumer _awsQueueConsumer;
    private readonly WorkerConfiguration _config;
	
    public Worker(IAwsQueueConsumer awsQueueConsumer, WorkerConfiguration config)
    {
        _awsQueueConsumer = awsQueueConsumer;
        _config = config;
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Parallel.ForEach(Enumerable.Range(1, _config.NumberOfInstances), index =>
        {
            _awsQueueConsumer.StartAsync($"{Assembly.GetExecutingAssembly().GetName().Name}({index})", _config.OrderQueueName, () => new OrderHandler(), stoppingToken);
        });

        return Task.CompletedTask;
    }
}
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
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _awsQueueConsumer.Start(_config.QueueName, _config.NumberOfThreads, () => new DefaultHandler(), stoppingToken);
        // while (!stoppingToken.IsCancellationRequested)
        // {
        //     Console.WriteLine($"hi - {_config.QueueName}");
        //     await Task.Delay(_config.DelayMilliseconds, stoppingToken);
        // }
    }
}
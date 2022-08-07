using Microsoft.Extensions.Hosting;
using Topica.Aws.Queues;

namespace Aws.Consumer.Host;

public class Worker : BackgroundService
{
    private readonly IAwsQueueConsumer _awsQueueConsumer;
    private readonly IWorkerConfiguration _config;
	
    public Worker(IAwsQueueConsumer awsQueueConsumer, IWorkerConfiguration config)
    {
        _awsQueueConsumer = awsQueueConsumer;
        _config = config;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("hi");
            await Task.Delay(_config.DelayMilliseconds, stoppingToken);
        }
    }
}
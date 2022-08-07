using System.Reflection;
using Aws.Consumer.Host.Handlers;
using Aws.Consumer.Host.Messages;
using Microsoft.Extensions.Hosting;
using Topica.Aws.Queues;
using Topica.Contracts;

namespace Aws.Consumer.Host;

public class Worker : BackgroundService
{
    private readonly IAwsQueueConsumer _awsQueueConsumer;
    private readonly WorkerConfiguration _config;
    private readonly IHandler<OrderMessage> _orderHandler;

    public Worker(IAwsQueueConsumer awsQueueConsumer, WorkerConfiguration config, IHandler<OrderMessage> orderHandler)
    {
        _awsQueueConsumer = awsQueueConsumer;
        _config = config;
        _orderHandler = orderHandler;
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Parallel.ForEach(Enumerable.Range(1, _config.NumberOfInstances), index =>
        {
            _awsQueueConsumer.StartAsync
            (
                $"{Assembly.GetExecutingAssembly().GetName().Name}({index})", 
                _config.OrderQueueName, 
                () => _orderHandler, 
                stoppingToken
            );
        });

        return Task.CompletedTask;
    }
}
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
    private readonly ConsumerSettings _consumerSettings;

    public Worker(IAwsQueueConsumer awsQueueConsumer,
        ConsumerSettings consumerSettings
    )
    {
        _awsQueueConsumer = awsQueueConsumer;
        _consumerSettings = consumerSettings;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var consumer in _consumerSettings.Consumers)
        {
            Parallel.ForEach(Enumerable.Range(1, consumer.NumberOfInstances), index =>
            {
                _awsQueueConsumer.StartAsync<OrderCreatedV1>($"{Assembly.GetExecutingAssembly().GetName().Name}-{consumer.Type}-({index})", consumer.QueueName, stoppingToken);
            });
        }

        return Task.CompletedTask;
    }
}
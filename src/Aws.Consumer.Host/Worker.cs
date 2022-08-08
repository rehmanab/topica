using System.Reflection;
using Aws.Consumer.Host.Messages;
using Microsoft.Extensions.Hosting;
using Topica.Aws.Queues;
using Topica.Contracts;

namespace Aws.Consumer.Host;

public class Worker : BackgroundService
{
    private readonly IQueueConsumer _awsQueueConsumer;
    private readonly ConsumerSettings _consumerSettings;

    public Worker(IQueueConsumer awsQueueConsumer,
        ConsumerSettings consumerSettings
    )
    {
        _awsQueueConsumer = awsQueueConsumer;
        _consumerSettings = consumerSettings;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Parallel.ForEach(Enumerable.Range(1, _consumerSettings.NumberOfInstancesPerConsumer), index =>
        {
            _awsQueueConsumer.StartAsync<OrderCreatedV1>($"{Assembly.GetExecutingAssembly().GetName().Name}-{nameof(OrderCreatedV1)}-({index})", _consumerSettings.OrderCreated.QueueName, stoppingToken);
            _awsQueueConsumer.StartAsync<CustomerCreatedV1>($"{Assembly.GetExecutingAssembly().GetName().Name}-{nameof(CustomerCreatedV1)}-({index})", _consumerSettings.CustomerCreated.QueueName, stoppingToken);
        });

        return Task.CompletedTask;
    }
}
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
    private readonly IHandler<OrderMessage> _orderHandler;
    private readonly IHandler<CustomerMessage> _customerHandler;

    public Worker(IAwsQueueConsumer awsQueueConsumer,
        ConsumerSettings consumerSettings,
        IHandler<OrderMessage> orderHandler,
        IHandler<CustomerMessage> customerHandler
    )
    {
        _awsQueueConsumer = awsQueueConsumer;
        _consumerSettings = consumerSettings;
        _orderHandler = orderHandler;
        _customerHandler = customerHandler;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var consumer in _consumerSettings.Consumers)
        {
            Parallel.ForEach(Enumerable.Range(1, consumer.NumberOfInstances), index =>
            {
                switch (consumer.Type)
                {
                    case HandlerType.Orders:
                        _awsQueueConsumer.StartAsync($"{Assembly.GetExecutingAssembly().GetName().Name}-{consumer.Type}-({index})", consumer.QueueName, () => _orderHandler, stoppingToken);
                        break;
                    case HandlerType.Customers:
                        _awsQueueConsumer.StartAsync($"{Assembly.GetExecutingAssembly().GetName().Name}-{consumer.Type}-({index})", consumer.QueueName, () => _customerHandler, stoppingToken);
                        break;
                    case HandlerType.Unknown:
                    default:
                        throw new Exception($"No handler for {consumer.QueueName}");
                }
            });
        }

        return Task.CompletedTask;
    }
}
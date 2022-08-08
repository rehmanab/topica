using System.Reflection;
using Aws.Consumer.Host.Messages;
using Microsoft.Extensions.Hosting;
using Topica.Aws.Settings;
using Topica.Contracts;

namespace Aws.Consumer.Host;

public class Worker : BackgroundService
{
    private readonly IConsumer _awsConsumer;
    private readonly ConsumerSettings _consumerSettings;

    public Worker(IConsumer awsConsumer, ConsumerSettings consumerSettings)
    {
        _awsConsumer = awsConsumer;
        _consumerSettings = consumerSettings;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Parallel.ForEach(Enumerable.Range(1, _consumerSettings.NumberOfInstancesPerConsumer), index =>
        {
            _awsConsumer.StartAsync<OrderCreatedV1>($"{Assembly.GetExecutingAssembly().GetName().Name}-{nameof(OrderCreatedV1)}-({index})", _consumerSettings.OrderCreated, stoppingToken);
            _awsConsumer.StartAsync<CustomerCreatedV1>($"{Assembly.GetExecutingAssembly().GetName().Name}-{nameof(CustomerCreatedV1)}-({index})", _consumerSettings.CustomerCreated, stoppingToken);
        });

        return Task.CompletedTask;
    }
}
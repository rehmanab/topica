using System.Reflection;
using Kafka.Consumer.Host.Messages;
using Microsoft.Extensions.Hosting;
using Topica.Contracts;
using Topica.Kafka.Settings;

namespace Kafka.Consumer.Host;

public class Worker : BackgroundService
{
    private readonly IConsumer _kafkaConsumer;
    private readonly ConsumerSettings _consumerSettings;

    public Worker(IConsumer kafkaConsumer, ConsumerSettings consumerSettings)
    {
        _kafkaConsumer = kafkaConsumer;
        _consumerSettings = consumerSettings;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Parallel.ForEach(Enumerable.Range(1, _consumerSettings.NumberOfInstancesPerConsumer), index =>
        {
            _kafkaConsumer.StartAsync<PlaceCreatedV1>($"{Assembly.GetExecutingAssembly().GetName().Name}-{nameof(PlaceCreatedV1)}-({index})", _consumerSettings.PlaceCreated, stoppingToken);
            _kafkaConsumer.StartAsync<PersonCreatedV1>($"{Assembly.GetExecutingAssembly().GetName().Name}-{nameof(PersonCreatedV1)}-({index})", _consumerSettings.PersonCreated, stoppingToken);
        });

        return Task.CompletedTask;
    }
}
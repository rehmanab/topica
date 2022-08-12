using System.Reflection;
using Kafka.Consumer.Host.Messages;
using Microsoft.Extensions.Hosting;
using Topica;
using Topica.Contracts;
using Topica.Kafka.Configuration;
using Topica.Kafka.Settings;
using Topica.Messages;
using Topica.Settings;

namespace Kafka.Consumer.Host;

public class Worker : BackgroundService
{
    private readonly ITopicCreatorFactory _topicCreatorFactory;
    private readonly ConsumerSettings _consumerSettings;

    public Worker(ITopicCreatorFactory topicCreatorFactory, ConsumerSettings consumerSettings)
    {
        _topicCreatorFactory = topicCreatorFactory;
        _consumerSettings = consumerSettings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await KafkaCreateTopic<PlaceCreatedV1>(_consumerSettings.PlaceCreated, _consumerSettings.NumberOfInstancesPerConsumer, stoppingToken);
        await KafkaCreateTopic<PersonCreatedV1>(_consumerSettings.PersonCreated, _consumerSettings.NumberOfInstancesPerConsumer, stoppingToken);
    }
    
    public async Task KafkaCreateTopic<T>(ConsumerItemSettings consumerItemSettings, int numberOfInstances, CancellationToken stoppingToken) where T : Message
    {
        var topicCreator = _topicCreatorFactory.Create(MessagingPlatform.Kafka);
        var consumer = await topicCreator.CreateTopic(new KafkaTopicConfiguration
        {
            TopicName = consumerItemSettings.Source,
            NumberOfPartitions = consumerItemSettings.NumberOfTopicPartitions
        });

        await consumer.ConsumeAsync<T>($"{Assembly.GetExecutingAssembly().GetName().Name}-{typeof(T).Name})", consumerItemSettings, numberOfInstances, stoppingToken);
    }
}
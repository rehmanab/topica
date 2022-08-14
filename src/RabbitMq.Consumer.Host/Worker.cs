using System.Reflection;
using Microsoft.Extensions.Hosting;
using RabbitMq.Consumer.Host.Messages.V1;
using Topica;
using Topica.Contracts;
using Topica.Messages;
using Topica.RabbitMq.Settings;
using Topica.Settings;

namespace RabbitMq.Consumer.Host;

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
        await RabbitMqCreateExchange<ItemPostedMessage>(_consumerSettings.ItemPosted, _consumerSettings.NumberOfInstancesPerConsumer, stoppingToken);
        await RabbitMqCreateExchange<ItemDeliveredMessage>(_consumerSettings.ItemDelivered, _consumerSettings.NumberOfInstancesPerConsumer, stoppingToken);
    }
    
    public async Task RabbitMqCreateExchange<T>(ConsumerItemSettings consumerItemSettings, int numberOfInstances, CancellationToken stoppingToken) where T : Message
    {
        var topicCreator = _topicCreatorFactory.Create(MessagingPlatform.RabbitMq);
        var consumer = await topicCreator.CreateTopic(new RabbitMqTopicSettings
        {
            TopicName = consumerItemSettings.Source,
            WithSubscribedQueues = new List<string>{ consumerItemSettings.Source }
        });

        await consumer.ConsumeAsync<T>($"{Assembly.GetExecutingAssembly().GetName().Name}-{typeof(T).Name})", consumerItemSettings, numberOfInstances, stoppingToken);
    }
}
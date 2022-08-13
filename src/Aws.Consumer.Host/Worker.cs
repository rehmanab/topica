using System.Reflection;
using Aws.Consumer.Host.Messages;
using Aws.Consumer.Host.Messages.V1;
using Microsoft.Extensions.Hosting;
using Topica;
using Topica.Aws.Configuration;
using Topica.Aws.Settings;
using Topica.Contracts;
using Topica.Messages;
using Topica.Settings;

namespace Aws.Consumer.Host;

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
        await AwsCreateTopicAndConsume<OrderCreatedMessage>(_consumerSettings.OrderCreated, _consumerSettings.NumberOfInstancesPerConsumer, stoppingToken);
        await AwsCreateTopicAndConsume<CustomerCreatedMessage>(_consumerSettings.CustomerCreated, _consumerSettings.NumberOfInstancesPerConsumer, stoppingToken);
    }

    public async Task AwsCreateTopicAndConsume<T>(ConsumerItemSettings consumerItemSettings, int numberOfInstances, CancellationToken stoppingToken) where T : Message
    {
        var topicCreator = _topicCreatorFactory.Create(MessagingPlatform.Aws);
        var consumer = await topicCreator.CreateTopic(new AwsTopicConfiguration
        {
            TopicName = consumerItemSettings.Source,
            WithSubscribedQueues = new List<string>
            {
                consumerItemSettings.Source
            },
            BuildWithErrorQueue = true,
            ErrorQueueMaxReceiveCount = 10,
            VisibilityTimeout = 30,
            IsFifoQueue = true,
            IsFifoContentBasedDeduplication = true
        });

        await consumer.ConsumeAsync<T>($"{Assembly.GetExecutingAssembly().GetName().Name}-{typeof(T).Name})", consumerItemSettings, numberOfInstances, stoppingToken);
    }
}
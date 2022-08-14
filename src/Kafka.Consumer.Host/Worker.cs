using System.Reflection;
using Microsoft.Extensions.Hosting;
using Topica;
using Topica.Contracts;
using Topica.Settings;

namespace Kafka.Consumer.Host;

public class Worker : BackgroundService
{
    private readonly ITopicCreatorFactory _topicCreatorFactory;
    private readonly IEnumerable<ConsumerSettings> _consumerSettings;

    public Worker(ITopicCreatorFactory topicCreatorFactory, IEnumerable<ConsumerSettings> consumerSettings)
    {
        _topicCreatorFactory = topicCreatorFactory;
        _consumerSettings = consumerSettings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var consumerSetting in _consumerSettings)
        {
            var topicCreator = _topicCreatorFactory.Create(MessagingPlatform.Kafka);
            var consumer = await topicCreator.CreateTopic(consumerSetting);

            var consumerName = $"{Assembly.GetExecutingAssembly().GetName().Name}-{consumerSetting.MessageToHandle}";
            await consumer.ConsumeAsync(consumerName, consumerSetting, stoppingToken);
        }
    }
}
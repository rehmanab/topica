using System.Reflection;
using Microsoft.Extensions.Hosting;
using Topica;
using Topica.Contracts;
using Topica.Settings;

namespace Pulsar.Consumer.Host;

public class Worker : BackgroundService
{
    private readonly ITopicProviderFactory _topicProviderFactory;
    private readonly IEnumerable<ConsumerSettings> _consumerSettings;

    public Worker(ITopicProviderFactory topicProviderFactory, IEnumerable<ConsumerSettings> consumerSettings)
    {
        _topicProviderFactory = topicProviderFactory;
        _consumerSettings = consumerSettings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var consumerSetting in _consumerSettings)
        {
            var topicCreator = _topicProviderFactory.Create(MessagingPlatform.Pulsar);
            var consumer = await topicCreator.CreateTopicAsync(consumerSetting);

            // Pulsar different consumer names will read the topic from the start and independently - i.e consumer name is like a consumer group
            var consumerName = $"{Assembly.GetExecutingAssembly().GetName().Name}-{consumerSetting.MessageToHandle}";
            await consumer.ConsumeAsync(consumerName, consumerSetting, stoppingToken);
        }
    }
}
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Topica;
using Topica.Contracts;
using Topica.Settings;

namespace RabbitMq.Consumer.Host;

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
            var topicCreator = _topicProviderFactory.Create(MessagingPlatform.RabbitMq);
            var consumer = await topicCreator.CreateTopicAsync(consumerSetting);

            var consumerName = $"{Assembly.GetExecutingAssembly().GetName().Name}-{consumerSetting.MessageToHandle}";
            await consumer.ConsumeAsync(consumerName, consumerSetting, stoppingToken);
        }
    }
}
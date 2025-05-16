using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;
using Topica.RabbitMq.Contracts;
using Topica.Settings;

namespace Topica.RabbitMq.Builders;

public class RabbitMqConsumerTopicFluentBuilder(IConsumer consumer) : IRabbitMqConsumerTopicFluentBuilder, IRabbitMqConsumerTopicBuilderWithTopic, IRabbitMqConsumerTopicBuilderWithQueues, IRabbitMqConsumerTopicBuilder
{
    private string _consumerName = null!;
    private string _topicName = null!;
    private string[] _queueNames = null!;

    public IRabbitMqConsumerTopicBuilderWithTopic WithConsumerName(string consumerName)
    {
        _consumerName = consumerName;
        return this;
    }

    public IRabbitMqConsumerTopicBuilderWithQueues WithTopicName(string topicName)
    {
        _topicName = topicName;
        return this;
    }

    public IRabbitMqConsumerTopicBuilder WithSubscribedQueues(params string[] queueNames)
    {
        _queueNames = queueNames;
        return this;
    }

    public async Task StartConsumingAsync<T>(string subscribeToQueueName, int numberOfInstances, CancellationToken cancellationToken = default) where T : class, IHandler
    {
        var instances = numberOfInstances switch
        {
            < 1 => 1,
            > 10 => 10,
            _ => numberOfInstances
        };

        var consumerSettings = new ConsumerSettings
        {
            Source = _topicName,
            WithSubscribedQueues = _queueNames,
            SubscribeToSource = subscribeToQueueName,
            NumberOfInstances = instances,
        };

        await consumer.ConsumeAsync<T>(_consumerName, consumerSettings, cancellationToken);
    }
}
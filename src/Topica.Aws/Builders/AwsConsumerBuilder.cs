using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Aws.Builders;

public class AwsConsumerBuilder(IConsumer consumer) : IConsumerFluentBuilder, IConsumerBuilderWithTopic, IConsumerBuilderWithQueues, IConsumerBuilder
{
    private string _consumerName = null!;
    private string _topicName = null!;
    private string[] _queueNames = null!;

    public IConsumerBuilderWithTopic WithConsumerName(string consumerName)
    {
        _consumerName = consumerName;
        return this;
    }

    public IConsumerBuilderWithQueues WithTopicName(string topicName)
    {
        _topicName = topicName;
        return this;
    }

    public IConsumerBuilder WithSubscribedQueues(params string[] queueNames)
    {
        _queueNames = queueNames;
        return this;
    }

    public async Task StartConsumingAsync<T>(CancellationToken cancellationToken) where T : class, IHandler
    {
        var consumerSettings = new ConsumerSettings
        {
            AwsIsFifoQueue = true,
            Source = _topicName,
            WithSubscribedQueues = _queueNames,
            SubscribeToSource = "ar-order_placement_warehouse.fifo",
            AwsBuildWithErrorQueue = true,
            AwsIsFifoContentBasedDeduplication = true,
            NumberOfInstances = 1,
            AwsMaximumNumberOfMessages = 10
        };

        await consumer.ConsumeAsync<T>(_consumerName, consumerSettings, cancellationToken);
    }

    public async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        var consumerSettings = new ConsumerSettings
        {
            MessageToHandle = "OrderCreatedMessageV1",
            AwsIsFifoQueue = true,
            Source = _topicName,
            WithSubscribedQueues = _queueNames,
            SubscribeToSource = "ar-order_placement_warehouse.fifo",
            AwsBuildWithErrorQueue = true,
            AwsIsFifoContentBasedDeduplication = true,
            NumberOfInstances = 1,
            AwsMaximumNumberOfMessages = 10
        };

        await consumer.ConsumeAsync(_consumerName, consumerSettings, cancellationToken);
    }
}
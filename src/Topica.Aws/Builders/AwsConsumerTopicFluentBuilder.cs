using System.Threading;
using System.Threading.Tasks;
using Topica.Aws.Contracts;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Aws.Builders;

public class AwsConsumerTopicFluentBuilder(IConsumer consumer) : IAwsConsumerTopicFluentBuilder, IAwsConsumerTopicBuilderWithTopic, IAwsConsumerTopicBuilderWithQueues, IAwsConsumerTopicBuilderWithFifo, IAwsConsumerTopicBuilder
{
    private string _consumerName = null!;
    private string _topicName = null!;
    private bool _buildErrorQueues;
    private string[] _queueNames = null!;
    private bool _isFifoQueue;
    private bool _isFifoContentBasedDeduplication;

    public IAwsConsumerTopicBuilderWithTopic WithConsumerName(string consumerName)
    {
        _consumerName = consumerName;
        return this;
    }

    public IAwsConsumerTopicBuilderWithQueues WithTopicName(string topicName)
    {
        _topicName = topicName;
        return this;
    }

    public IAwsConsumerTopicBuilderWithFifo WithSubscribedQueues(bool buildErrorQueues, params string[] queueNames)
    {
        _buildErrorQueues = buildErrorQueues;
        _queueNames = queueNames;
        return this;
    }

    public IAwsConsumerTopicBuilder WithFifoSettings(bool isFifoQueue, bool isFifoContentBasedDeduplication)
    {
        _isFifoQueue = isFifoQueue;
        _isFifoContentBasedDeduplication = isFifoContentBasedDeduplication;
        return this;
    }

    public async Task StartConsumingAsync<T>(string subscribeToQueueName, int numberOfInstances, int receiveMaximumNumberOfMessages, CancellationToken cancellationToken = default) where T : class, IHandler
    {
        var instances = numberOfInstances switch
        {
            < 1 => 1,
            > 10 => 10,
            _ => numberOfInstances
        };

        var consumerSettings = new ConsumerSettings
        {
            AwsIsFifoQueue = _isFifoQueue,
            Source = _topicName,
            AwsWithSubscribedQueues = _queueNames,
            SubscribeToSource = _isFifoQueue && !subscribeToQueueName.EndsWith(".fifo") ? $"{subscribeToQueueName}.fifo" : subscribeToQueueName,
            AwsBuildWithErrorQueue = _buildErrorQueues,
            AwsIsFifoContentBasedDeduplication = _isFifoContentBasedDeduplication,
            NumberOfInstances = instances,
            AwsReceiveMaximumNumberOfMessages = receiveMaximumNumberOfMessages
        };

        await consumer.ConsumeAsync<T>(_consumerName, consumerSettings, cancellationToken);
    }
}
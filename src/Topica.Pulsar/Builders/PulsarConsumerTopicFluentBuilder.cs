using System;
using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;
using Topica.Pulsar.Contracts;
using Topica.Settings;

namespace Topica.Pulsar.Builders;

public class PulsarConsumerTopicFluentBuilder(IConsumer consumer) : IPulsarConsumerTopicFluentBuilder, IPulsarConsumerTopicBuilderWithTopic, IPulsarConsumerTopicBuilderWithQueues, IPulsarConsumerTopicBuilderWithConfiguration, IPulsarConsumerTopicBuilderWithOptions, IPulsarConsumerTopicBuilder
{
    private string _consumerName = null!;
    private string _topicName = null!;
    private string _consumerGroup = null!;
    private string _tenant = null!;
    private string _namespace = null!;
    private bool _startNewConsumerEarliest;

    public IPulsarConsumerTopicBuilderWithTopic WithConsumerName(string consumerName)
    {
        _consumerName = consumerName;
        return this;
    }

    public IPulsarConsumerTopicBuilderWithQueues WithTopicName(string topicName)
    {
        _topicName = topicName;
        return this;
    }

    public IPulsarConsumerTopicBuilderWithConfiguration WithConsumerGroup(string consumerGroup)
    {
        _consumerGroup = consumerGroup;
        return this;
    }

    public IPulsarConsumerTopicBuilderWithOptions WithConfiguration(string tenant, string @namespace)
    {
        _tenant = tenant;
        _namespace = @namespace;
        return this;
    }
        
    public IPulsarConsumerTopicBuilder WithTopicOptions(bool startNewConsumerEarliest)
    {
        _startNewConsumerEarliest = startNewConsumerEarliest;
        return this;
    }

    public async Task StartConsumingAsync<T>(int numberOfInstances, CancellationToken cancellationToken = default) where T : class, IHandler
    {
        var instances = numberOfInstances switch
        {
            < 1 => 1,
            > 10 => 10,
            _ => numberOfInstances
        };

        var consumerSettings = new ConsumerSettings
        {
            Source = _topicName ?? throw new ApplicationException("Pulsar topic name is not set."),
            PulsarTenant = _tenant ?? throw new ApplicationException("Pulsar tenant is not set."),
            PulsarNamespace = _namespace ?? throw new ApplicationException("Pulsar namespace is not set."),
            PulsarConsumerGroup = _consumerGroup ?? throw new ApplicationException("Pulsar consumer group is not set."),
            PulsarStartNewConsumerEarliest = _startNewConsumerEarliest,
            NumberOfInstances = instances
        };

        await consumer.ConsumeAsync<T>(_consumerName, consumerSettings, cancellationToken);
    }
}
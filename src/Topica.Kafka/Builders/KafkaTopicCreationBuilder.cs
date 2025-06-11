using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Kafka.Contracts;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Kafka.Builders;

public class KafkaTopicCreationBuilder(
    IPollyRetryService pollyRetryService,
    ITopicProviderFactory topicProviderFactory, 
    ILogger<KafkaTopicCreationBuilder> logger) 
    : IKafkaTopicCreationBuilder, IKafkaTopicBuilderWithTopicName, IKafkaTopicBuilderWithQueues, IKafkaTopicBuilderWithTopicSettings, IKafkaTopicBuilderWithBootstrapServers, IKafkaTopicBuilder
{
    private string _workerName = null!;
    private string _topicName = null!;
    private string _consumerGroup = null!;
    private bool? _startFromEarliestMessages;
    private int? _numberOfTopicPartitions;
    private string[] _bootstrapServers = null!;
    private int? _numberOfInstances;

    public IKafkaTopicBuilderWithTopicName WithWorkerName(string workerName)
    {
        _workerName = workerName;
        return this;
    }

    public IKafkaTopicBuilderWithQueues WithTopicName(string topicName)
    {
        _topicName = topicName;
        return this;
    }

    public IKafkaTopicBuilderWithTopicSettings WithConsumerGroup(string consumerGroup)
    {
        _consumerGroup = consumerGroup;
        return this;
    }

    public IKafkaTopicBuilderWithBootstrapServers WithTopicSettings(bool? startFromEarliestMessages, int? numberOfTopicPartitions)
    {
        _startFromEarliestMessages = startFromEarliestMessages;
        _numberOfTopicPartitions = numberOfTopicPartitions;
        return this;
    }

    public IKafkaTopicBuilder WithBootstrapServers(params string[] bootstrapServers)
    {
        _bootstrapServers = bootstrapServers;
        return this; 
    }

    public IKafkaTopicBuilder WithConsumeSettings(int? numberOfInstances)
    {
        _numberOfInstances = numberOfInstances;
        return this;
    }

    public async Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken)
    {
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.Kafka);
        var messagingSettings = GetMessagingSettings();
        
        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for consumer: {Name} to Source: {MessagingSettings}", MessagingPlatform.Kafka, _workerName, messagingSettings.Source);
        await pollyRetryService.WaitAndRetryAsync<Exception>
        (
            30,
            _ => TimeSpan.FromSeconds(10),
            (delegateResult, ts, index, context) => logger.LogWarning("**** RETRY: {Name}:  Retry attempt: {RetryAttempt} - Retry in {RetryDelayTotalSeconds} - Error ({ExceptionType}) Message: {Result}", nameof(KafkaTopicCreationBuilder), index, ts, delegateResult.GetType(), delegateResult.Message ?? "Error creating queue."),
            () => topicProvider.CreateTopicAsync(messagingSettings),
            false
        );

        return await topicProvider.ProvideConsumerAsync(messagingSettings);
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {
        var messagingSettings = GetMessagingSettings();

        var topicProvider = topicProviderFactory.Create(MessagingPlatform.Kafka);
        
        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for producer: {Name} to Source: {MessagingSettings}", MessagingPlatform.Kafka, _workerName, messagingSettings.Source);
        await pollyRetryService.WaitAndRetryAsync<Exception>
        (
            30,
            _ => TimeSpan.FromSeconds(10),
            (delegateResult, ts, index, context) => logger.LogWarning("**** RETRY: {Name}:  Retry attempt: {RetryAttempt} - Retry in {RetryDelayTotalSeconds} - Error ({ExceptionType}) Message: {Result}", nameof(KafkaTopicCreationBuilder), index, ts, delegateResult.GetType(), delegateResult.Message ?? "Error creating queue."),
            () => topicProvider.CreateTopicAsync(messagingSettings),
            false
        );
        
        return await topicProvider.ProvideProducerAsync(_workerName, messagingSettings);
    }

    private MessagingSettings GetMessagingSettings()
    {
        return new MessagingSettings
        {
            WorkerName = _workerName,
            Source = _topicName,
            KafkaConsumerGroup = _consumerGroup,
            KafkaStartFromEarliestMessages = _startFromEarliestMessages ?? false,
            KafkaNumberOfTopicPartitions = _numberOfTopicPartitions ?? 6,
            KafkaBootstrapServers = _bootstrapServers,
            NumberOfInstances = _numberOfInstances ?? 1
        };
    }
}
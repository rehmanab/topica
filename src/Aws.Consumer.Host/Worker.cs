using Aws.Consumer.Host.Handlers.V1;
using Aws.Consumer.Host.Messages.V1;
using Aws.Consumer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Topica.Aws.Contracts;

namespace Aws.Consumer.Host;

public class Worker(IAwsTopicFluentBuilder builder, AwsConsumerSettings settings) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await (await builder
                .WithWorkerName(nameof(OrderPlacedMessageV1))
                .WithTopicName(settings.OrderPlacedTopicSettings.Source)
                .WithSubscribedQueues(
                    settings.OrderPlacedTopicSettings.SubscribeToSource,
                    settings.OrderPlacedTopicSettings.WithSubscribedQueues
                )
                .WithErrorQueueSettings(
                    settings.OrderPlacedTopicSettings.BuildWithErrorQueue,
                    settings.OrderPlacedTopicSettings.ErrorQueueMaxReceiveCount
                )
                .WithFifoSettings(
                    settings.OrderPlacedTopicSettings.IsFifoQueue,
                    settings.OrderPlacedTopicSettings.IsFifoContentBasedDeduplication
                )
                .WithTemporalSettings(
                    settings.OrderPlacedTopicSettings.MessageVisibilityTimeoutSeconds,
                    settings.OrderPlacedTopicSettings.QueueMessageDelaySeconds,
                    settings.OrderPlacedTopicSettings.QueueMessageRetentionPeriodSeconds,
                    settings.OrderPlacedTopicSettings.QueueReceiveMessageWaitTimeSeconds
                )
                .WithQueueSettings(settings.OrderPlacedTopicSettings.QueueMaximumMessageSize)
                .BuildConsumerAsync(
                    settings.OrderPlacedTopicSettings.NumberOfInstances,
                    settings.OrderPlacedTopicSettings.QueueReceiveMaximumNumberOfMessages,
                    stoppingToken
                ))
            .ConsumeAsync<OrderPlacedMessageHandlerV1>(stoppingToken);

        await (await builder
                .WithWorkerName(nameof(CustomerCreatedMessageV1))
                .WithTopicName(settings.CustomerCreatedTopicSettings.Source)
                .WithSubscribedQueues(
                    settings.CustomerCreatedTopicSettings.SubscribeToSource,
                    settings.CustomerCreatedTopicSettings.WithSubscribedQueues
                )
                .WithErrorQueueSettings(
                    settings.CustomerCreatedTopicSettings.BuildWithErrorQueue,
                    settings.CustomerCreatedTopicSettings.ErrorQueueMaxReceiveCount
                )
                .WithFifoSettings(
                    settings.CustomerCreatedTopicSettings.IsFifoQueue,
                    settings.CustomerCreatedTopicSettings.IsFifoContentBasedDeduplication
                )
                .WithTemporalSettings(
                    settings.CustomerCreatedTopicSettings.MessageVisibilityTimeoutSeconds,
                    settings.CustomerCreatedTopicSettings.QueueMessageDelaySeconds,
                    settings.CustomerCreatedTopicSettings.QueueMessageRetentionPeriodSeconds,
                    settings.CustomerCreatedTopicSettings.QueueReceiveMessageWaitTimeSeconds
                )
                .WithQueueSettings(settings.CustomerCreatedTopicSettings.QueueMaximumMessageSize)
                .BuildConsumerAsync(
                    settings.CustomerCreatedTopicSettings.NumberOfInstances,
                    settings.CustomerCreatedTopicSettings.QueueReceiveMaximumNumberOfMessages,
                    stoppingToken
                ))
            .ConsumeAsync<CustomerCreatedMessageHandlerV1>(stoppingToken);
    }
}
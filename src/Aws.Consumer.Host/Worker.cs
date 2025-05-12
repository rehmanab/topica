using System.Reflection;
using Aws.Consumer.Host.Handlers.V1;
using Aws.Consumer.Host.Messages.V1;
using Aws.Consumer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Topica.Aws.Contracts;
using Topica.Contracts;
using Topica.Settings;

namespace Aws.Consumer.Host;

public class Worker(IConsumer consumer, IAwsConsumerTopicFluentBuilder awsConsumerTopicFluentBuilder, AwsConsumerSettings awsConsumerSettings) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // foreach (var consumerSetting in consumerSettingsList)
        // {
        //     await consumer.ConsumeAsync($"{Assembly.GetExecutingAssembly().GetName().Name}-{consumerSetting.MessageToHandle}", consumerSetting, stoppingToken);
        // }
        
        await awsConsumerTopicFluentBuilder
            .WithConsumerName(nameof(OrderPlacedMessageV1))
            .WithTopicName(awsConsumerSettings.OrderPlacedTopicSettings.Source)
            .WithSubscribedQueues(true, awsConsumerSettings.OrderPlacedTopicSettings.WithSubscribedQueues)
            .WithFifoSettings(true, true)
            .StartConsumingAsync<OrderPlacedMessageHandlerV1>(
                awsConsumerSettings.OrderPlacedTopicSettings.SubscribeToSource,
                awsConsumerSettings.OrderPlacedTopicSettings.NumberOfInstances,
                awsConsumerSettings.OrderPlacedTopicSettings.AwsReceiveMaximumNumberOfMessages,
                stoppingToken
            );
        
        await awsConsumerTopicFluentBuilder
            .WithConsumerName(nameof(CustomerCreatedMessageV1))
            .WithTopicName(awsConsumerSettings.CustomerCreatedTopicSettings.Source)
            .WithSubscribedQueues(true, awsConsumerSettings.CustomerCreatedTopicSettings.WithSubscribedQueues)
            .WithFifoSettings(true, true)
            .StartConsumingAsync<CustomerCreatedMessageHandlerV1>(
                awsConsumerSettings.CustomerCreatedTopicSettings.SubscribeToSource,
                awsConsumerSettings.CustomerCreatedTopicSettings.NumberOfInstances,
                awsConsumerSettings.CustomerCreatedTopicSettings.AwsReceiveMaximumNumberOfMessages,
                stoppingToken
            );
    }
}
using Aws.Consumer.Host.Handlers.V1;
using Aws.Consumer.Host.Messages.V1;
using Microsoft.Extensions.Hosting;
using Topica.Aws.Contracts;
using Topica.Aws.Settings;

namespace Aws.Consumer.Host;

public class Worker(IAwsConsumerTopicFluentBuilder builder, AwsConsumerSettings settings) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if(settings.OrderPlacedTopicSettings is null)
        {
            throw new ApplicationException($"{nameof(settings.OrderPlacedTopicSettings)} cannot be null.");
        }
        
        if(settings.CustomerCreatedTopicSettings is null)
        {
            throw new ApplicationException($"{nameof(settings.CustomerCreatedTopicSettings)} cannot be null.");
        }
        
        await builder
            .WithConsumerName(nameof(OrderPlacedMessageV1))
            .WithTopicName(settings.OrderPlacedTopicSettings!.Source!)
            .WithSubscribedQueues(true, settings.OrderPlacedTopicSettings!.WithSubscribedQueues!)
            .WithFifoSettings(true, true)
            .StartConsumingAsync<OrderPlacedMessageHandlerV1>(
                settings.OrderPlacedTopicSettings!.SubscribeToSource!,
                settings.OrderPlacedTopicSettings.NumberOfInstances,
                settings.OrderPlacedTopicSettings.ReceiveMaximumNumberOfMessages,
                stoppingToken
            );
        
        await builder
            .WithConsumerName(nameof(CustomerCreatedMessageV1))
            .WithTopicName(settings.CustomerCreatedTopicSettings!.Source!)
            .WithSubscribedQueues(true, settings.CustomerCreatedTopicSettings!.WithSubscribedQueues!)
            .WithFifoSettings(true, true)
            .StartConsumingAsync<CustomerCreatedMessageHandlerV1>(
                settings.CustomerCreatedTopicSettings!.SubscribeToSource!,
                settings.CustomerCreatedTopicSettings.NumberOfInstances,
                settings.CustomerCreatedTopicSettings.ReceiveMaximumNumberOfMessages,
                stoppingToken
            );
    }
}
using Microsoft.Extensions.Hosting;
using RabbitMq.Consumer.Host.Handlers.V1;
using RabbitMq.Consumer.Host.Messages.V1;
using RabbitMq.Consumer.Host.Settings;
using Topica.RabbitMq.Contracts;

namespace RabbitMq.Consumer.Host;

public class Worker(IRabbitMqConsumerTopicFluentBuilder builder, RabbitMqConsumerSettings settings) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await builder
            .WithConsumerName(nameof(ItemDeliveredMessageV1))
            .WithTopicName(settings.ItemDeliveredTopicSettings!.Source!)
            .WithSubscribedQueues(settings.ItemDeliveredTopicSettings!.WithSubscribedQueues!)
            .StartConsumingAsync<ItemDeliveredMessageHandlerV1>(
                settings.ItemDeliveredTopicSettings!.SubscribeToSource!,
                settings.ItemDeliveredTopicSettings.NumberOfInstances,
                stoppingToken
            );
        
        await builder
            .WithConsumerName(nameof(ItemPostedMessageV1))
            .WithTopicName(settings.ItemPostedTopicSettings!.Source!)
            .WithSubscribedQueues(settings.ItemPostedTopicSettings!.WithSubscribedQueues!)
            .StartConsumingAsync<ItemPostedMessageHandlerV1>(
                settings.ItemPostedTopicSettings!.SubscribeToSource!,
                settings.ItemPostedTopicSettings.NumberOfInstances,
                stoppingToken
            );
    }
}
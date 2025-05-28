using Microsoft.Extensions.Hosting;
using RabbitMq.Consumer.Host.Handlers.V1;
using RabbitMq.Consumer.Host.Messages.V1;
using RabbitMq.Consumer.Host.Settings;
using Topica.RabbitMq.Contracts;

namespace RabbitMq.Consumer.Host;

public class Worker(IRabbitMqTopicFluentBuilder builder, RabbitMqConsumerSettings settings) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await (await builder
                .WithWorkerName(nameof(ItemDeliveredMessageV1))
                .WithTopicName(settings.ItemDeliveredTopicSettings.Source)
                .WithSubscribedQueues(
                    settings.ItemDeliveredTopicSettings.SubscribeToSource,
                    settings.ItemDeliveredTopicSettings.WithSubscribedQueues
                )
                .BuildConsumerAsync(
                    settings.ItemDeliveredTopicSettings.NumberOfInstances,
                    stoppingToken
                ))
            .ConsumeAsync<ItemDeliveredMessageHandlerV1>(stoppingToken);

        await (await builder
                .WithWorkerName(nameof(ItemPostedMessageV1))
                .WithTopicName(settings.ItemPostedTopicSettings.Source)
                .WithSubscribedQueues(
                    settings.ItemPostedTopicSettings.SubscribeToSource,
                    settings.ItemPostedTopicSettings.WithSubscribedQueues
                )
                .BuildConsumerAsync(
                    settings.ItemPostedTopicSettings.NumberOfInstances,
                    stoppingToken
                ))
            .ConsumeAsync<ItemPostedMessageHandlerV1>(stoppingToken);
    }
}
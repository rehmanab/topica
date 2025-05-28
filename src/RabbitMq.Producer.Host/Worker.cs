using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMq.Producer.Host.Messages.V1;
using RabbitMq.Producer.Host.Settings;
using RandomNameGeneratorLibrary;
using Topica.RabbitMq.Contracts;

namespace RabbitMq.Producer.Host;

public class Worker(IRabbitMqTopicFluentBuilder builder, RabbitMqProducerSettings settings, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const string workerName = $"{nameof(ItemDeliveredMessageV1)}_rabbitmq_producer_host_1";

        var itemDeliveredProducer = await builder
            .WithWorkerName(workerName)
            .WithTopicName(settings.ItemDeliveredTopicSettings.Source)
            .WithSubscribedQueues(
                settings.ItemDeliveredTopicSettings.SubscribeToSource,
                settings.ItemDeliveredTopicSettings.WithSubscribedQueues
            )
            .BuildProducerAsync(stoppingToken);

        var count = 1;
        while(!stoppingToken.IsCancellationRequested)
        {
            var message = new ItemDeliveredMessageV1 { ConversationId = Guid.NewGuid(), ItemId = count, ItemName = Random.Shared.GenerateRandomPlaceName(), Type = nameof(ItemDeliveredMessageV1) };
            
            await itemDeliveredProducer.ProduceAsync(settings.ItemDeliveredTopicSettings.Source, message, cancellationToken: stoppingToken);

            logger.LogInformation("Produced message to {MessagingSettingsSource}: {MessageIdName}", settings.ItemDeliveredTopicSettings.Source, $"{message.ItemId} : {message.ItemName}");
            count++;
            
            await Task.Delay(1000, stoppingToken);
        }

        await itemDeliveredProducer.DisposeAsync();

        logger.LogInformation("Finished!");
    }
}
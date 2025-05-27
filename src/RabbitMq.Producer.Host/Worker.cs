using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMq.Producer.Host.Messages.V1;
using RandomNameGeneratorLibrary;
using Topica.Contracts;
using Topica.Settings;

namespace RabbitMq.Producer.Host;

public class Worker(IProducerBuilder producerBuilder, ProducerSettings producerSettings, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var producer = await producerBuilder.BuildProducerAsync<IChannel>(null, producerSettings, stoppingToken);

        var count = 1;
        while(!stoppingToken.IsCancellationRequested)
        {
            var message = new ItemDeliveredMessageV1{ConversationId = Guid.NewGuid(), ItemId = count, ItemName = Random.Shared.GenerateRandomPlaceName(), Type = nameof(ItemDeliveredMessageV1)};
            await producer.BasicPublishAsync("item_delivered_v1_exchange", "", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)), cancellationToken: stoppingToken);
            count++;
    
            logger.LogInformation("Produced message to {ProducerSettingsSource}: {Count}", producerSettings.Source, count);
    
            await Task.Delay(1000, stoppingToken);
        }

        producer.Dispose();

        logger.LogInformation("Finished!");
    }
}
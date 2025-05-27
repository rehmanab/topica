using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulsar.Client.Api;
using Pulsar.Producer.Host.Messages.V1;
using RandomNameGeneratorLibrary;
using Topica.Contracts;
using Topica.Settings;

namespace Pulsar.Producer.Host;

public class Worker(IProducerBuilder producerBuilder, ProducerSettings producerSettings, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var producer = await producerBuilder.BuildProducerAsync<IProducer<byte[]>>("pulsar_producer_host_1", producerSettings, stoppingToken);
   
        var count = 1;
        while(!stoppingToken.IsCancellationRequested)
        {
            var message = new DataSentMessageV1{ConversationId = Guid.NewGuid(), DataId = count, DataName = Random.Shared.GenerateRandomMaleFirstAndLastName(), Type = nameof(DataSentMessageV1)};
            await producer.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
            count++;
    
            logger.LogInformation("Produced message to {ProducerSettingsSource}: {Count}", producerSettings?.Source, count);
    
            await Task.Delay(1000, stoppingToken);
        }

        await producer.DisposeAsync();

        logger.LogInformation("Finished: {Count} messages sent", count);
    }
}
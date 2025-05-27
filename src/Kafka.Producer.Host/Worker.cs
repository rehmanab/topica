using Confluent.Kafka;
using Kafka.Producer.Host.Messages.V1;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RandomNameGeneratorLibrary;
using Topica.Contracts;
using Topica.Settings;

namespace Kafka.Producer.Host;

public class Worker(IProducerBuilder producerBuilder, ProducerSettings producerSettings, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var producer = await producerBuilder.BuildProducerAsync<IProducer<string, string>>("kafka_producer_host_1", producerSettings, stoppingToken);
        
        var count = 1;
        var personNameGenerator = new PersonNameGenerator();
        while(true)
        {
            var name = personNameGenerator.GenerateRandomFirstAndLastName();
            var result = await producer.ProduceAsync(producerSettings?.Source, new Message<string, string>
            {
                Key = name,
                Value = JsonConvert.SerializeObject(new PersonCreatedMessageV1 { PersonId = count, PersonName = name, ConversationId = Guid.NewGuid(), Type = nameof(PersonCreatedMessageV1) })
            }, stoppingToken);
            // FLush will wait for all messages sent to be confirmed/delivered, so dont use after each Produce()
            //producer.Flush(TimeSpan.FromSeconds(2));

            count++;
    
            logger.LogInformation("Produced message to {ProducerSettingsSource}: {Count}", producerSettings?.Source, count);
    
            await Task.Delay(1000, stoppingToken);
        }

        producer.Dispose();
    }
}
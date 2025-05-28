using Kafka.Producer.Host.Messages.V1;
using Kafka.Producer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RandomNameGeneratorLibrary;
using Topica.Kafka.Contracts;

namespace Kafka.Producer.Host;

public class Worker(IKafkaTopicFluentBuilder builder, KafkaProducerSettings settings, KafkaHostSettings hostSettings, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const string workerName = $"{nameof(PersonCreatedMessageV1)}_kafka_producer_host_1";
        
        var personCreatedProducer = await builder
            .WithWorkerName(workerName)
            .WithTopicName(settings.PersonCreatedTopicSettings.Source)
            .WithConsumerGroup(settings.PersonCreatedTopicSettings.ConsumerGroup)
            .WithTopicSettings(settings.PersonCreatedTopicSettings.StartFromEarliestMessages, settings.PersonCreatedTopicSettings.NumberOfTopicPartitions)
            .WithBootstrapServers(hostSettings.BootstrapServers)
            .BuildProducerAsync(stoppingToken);
        
        var count = 1;
        var personNameGenerator = new PersonNameGenerator();
        while(!stoppingToken.IsCancellationRequested)
        {
            var name = personNameGenerator.GenerateRandomFirstAndLastName();
            var message = new PersonCreatedMessageV1 { PersonId = count, PersonName = name, ConversationId = Guid.NewGuid(), Type = nameof(PersonCreatedMessageV1) };

            await personCreatedProducer.ProduceAsync(settings.PersonCreatedTopicSettings.Source, message, new Dictionary<string, string> { { "bootstrapservers", string.Join(",", hostSettings.BootstrapServers) } }, stoppingToken);

            logger.LogInformation("Produced message to {MessagingSettingsSource}: {MessageIdName}", settings.PersonCreatedTopicSettings.Source, $"{message.PersonId} : {message.PersonName}");
            
            count++;

            await Task.Delay(1000, stoppingToken);
        }

        await personCreatedProducer.DisposeAsync();
    }
}
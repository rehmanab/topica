using System;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Kafka.Providers
{
    public class KafkaTopicProvider(ILogger<KafkaTopicProvider> logger) : ITopicProvider
    {
        public MessagingPlatform MessagingPlatform => MessagingPlatform.Kafka;
        
        public async Task CreateTopicAsync(ConsumerSettings settings)
        {
            await CreateTopicAsync(settings.Source!, settings.KafkaBootstrapServers!, settings.KafkaNumberOfTopicPartitions!);
        }

        public async Task CreateTopicAsync(ProducerSettings settings)
        {
            await CreateTopicAsync(settings.Source, settings.KafkaBootstrapServers, settings.KafkaNumberOfTopicPartitions);
        }
        
        private async Task CreateTopicAsync(string source, string[] kafkaBootstrapServers, int? kafkaNumberOfTopicPartitions)
        {
            using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = string.Join(",", kafkaBootstrapServers) }).Build();
            
            try
            {
                var meta = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

                if (meta.Topics.Any(x => string.Equals(source, x.Topic, StringComparison.CurrentCultureIgnoreCase)))
                {
                    logger.LogInformation("{KafkaTopicProviderName}.{CreateTopicAsyncName} topic {Source} already exists!", nameof(KafkaTopicProvider), nameof(CreateTopicAsync), source);
                    return;
                }
                
                await adminClient.CreateTopicsAsync([
                    new TopicSpecification { Name = source, ReplicationFactor = 1, NumPartitions = kafkaNumberOfTopicPartitions ?? 1 }
                ]);
                
                logger.LogInformation("{KafkaTopicProviderName}.{CreateTopicAsyncName}: Created topic {Source}", nameof(KafkaTopicProvider), nameof(CreateTopicAsync), source);
            }
            catch (CreateTopicsException ex)
            {
                logger.LogError(ex, "{KafkaTopicProviderName}.{CreateTopicAsyncName}: An error occured creating topic {Topic}: {ErrorReason}", nameof(KafkaTopicProvider), nameof(CreateTopicAsync), ex.Results[0].Topic, ex.Results[0].Error.Reason);
                
                throw;
            }
        }
    }
}
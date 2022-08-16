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
    public class KafkaTopicProvider : ITopicProvider
    {
        private readonly IConsumer _consumer;
        private readonly IProducerBuilder _producerBuilder;
        private readonly ILogger<KafkaTopicProvider> _logger;

        public KafkaTopicProvider(IConsumer consumer, IProducerBuilder producerBuilder, ILogger<KafkaTopicProvider> logger)
        {
            _consumer = consumer;
            _producerBuilder = producerBuilder;
            _logger = logger;
        }
        
        public MessagingPlatform MessagingPlatform => MessagingPlatform.Kafka;
        
        public async Task<IConsumer> CreateTopicAsync(ConsumerSettings settings)
        {
            await CreateTopicAsync(settings.Source, settings.KafkaBootstrapServers, settings.KafkaNumberOfTopicPartitions);

            return _consumer;
        }

        public async Task<IProducerBuilder> CreateTopicAsync(ProducerSettings settings)
        {
            await CreateTopicAsync(settings.Source, settings.KafkaBootstrapServers, settings.KafkaNumberOfTopicPartitions);

            return _producerBuilder;
        }

        public IConsumer GetConsumer()
        {
            return _consumer;
        }

        public IProducerBuilder GetProducerBuilder()
        {
            return _producerBuilder;
        }
        
        private async Task CreateTopicAsync(string source, string[] kafkaBootstrapServers, int kafkaNumberOfTopicPartitions)
        {
            using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = string.Join(",", kafkaBootstrapServers) }).Build();
            
            try
            {
                var meta = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

                if (meta.Topics.Any(x => string.Equals(source, x.Topic, StringComparison.CurrentCultureIgnoreCase)))
                {
                    _logger.LogInformation($"{nameof(KafkaTopicProvider)}.{nameof(CreateTopicAsync)} topic {source} already exists!");
                    return;
                }
                
                await adminClient.CreateTopicsAsync(new[] 
                {
                    new TopicSpecification { Name = source, ReplicationFactor = 1, NumPartitions = kafkaNumberOfTopicPartitions } 
                });
                
                _logger.LogInformation($"{nameof(KafkaTopicProvider)}.{nameof(CreateTopicAsync)}: Created topic {source}");
            }
            catch (CreateTopicsException ex)
            {
                _logger.LogError(ex, $"{nameof(KafkaTopicProvider)}.{nameof(CreateTopicAsync)}: An error occured creating topic {ex.Results[0].Topic}: {ex.Results[0].Error.Reason}");
                
                throw;
            }
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Kafka.Configuration;
using Topica.Kafka.Settings;
using Topica.Topics;

namespace Topica.Kafka.Topics
{
    public class KafkaTopicCreator : ITopicCreator
    {
        private readonly KafkaSettings _kafkaSettings;
        private readonly IConsumer _consumer;
        private readonly ILogger<KafkaTopicCreator> _logger;

        public KafkaTopicCreator(KafkaSettings kafkaSettings, IConsumer consumer, ILogger<KafkaTopicCreator> logger)
        {
            _kafkaSettings = kafkaSettings;
            _consumer = consumer;
            _logger = logger;
        }
        
        public MessagingPlatform MessagingPlatform => MessagingPlatform.Kafka;
        
        public async Task<IConsumer> CreateTopic(TopicConfigurationBase configuration)
        {
            var config = configuration as KafkaTopicConfiguration;

            if (config == null)
            {
                throw new Exception($"{nameof(KafkaTopicCreator)}.{nameof(CreateTopic)} needs an {nameof(KafkaTopicConfiguration)} ");
            }

            using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = string.Join(",", _kafkaSettings.BootstrapServers) }).Build();
            
            try
            {
                var meta = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

                if (meta.Topics.Any(x => string.Equals(config.TopicName, x.Topic, StringComparison.CurrentCultureIgnoreCase)))
                {
                    _logger.LogInformation($"{nameof(KafkaTopicCreator)}.{nameof(CreateTopic)} topic {config.TopicName} already exists!");
                    return _consumer;
                }
                
                await adminClient.CreateTopicsAsync(new[] 
                {
                    new TopicSpecification { Name = config.TopicName, ReplicationFactor = 1, NumPartitions = config.NumberOfPartitions } 
                });
                
                _logger.LogInformation("Created topic {Topic}", config.TopicName);

                return _consumer;
            }
            catch (CreateTopicsException ex)
            {
                _logger.LogError(ex, $"An error occured creating topic {ex.Results[0].Topic}: {ex.Results[0].Error.Reason}");
                
                throw;
            }
        }
    }
}
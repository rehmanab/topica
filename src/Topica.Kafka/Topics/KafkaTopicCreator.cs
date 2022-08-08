using System;
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
        private readonly ILogger<KafkaTopicCreator> _logger;

        public KafkaTopicCreator(KafkaSettings kafkaSettings, ILogger<KafkaTopicCreator> logger)
        {
            _kafkaSettings = kafkaSettings;
            _logger = logger;
        }
        
        public MessagingPlatform MessagingPlatform => MessagingPlatform.Kafka;
        
        public async Task<string> CreateTopic(TopicConfigurationBase configuration)
        {
            var config = configuration as KafkaTopicConfiguration;

            if (config == null)
            {
                throw new Exception($"{nameof(KafkaTopicCreator)}.{nameof(CreateTopic)} needs an {nameof(KafkaTopicConfiguration)} ");
            }

            using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = string.Join(",", _kafkaSettings.BootstrapServers) }).Build();
            try
            {
                await adminClient.CreateTopicsAsync(new[] 
                {
                    new TopicSpecification { Name = config.TopicName, ReplicationFactor = 1, NumPartitions = config.NumberOfPartitions } 
                });
                
                return config.TopicName;
            }
            catch (CreateTopicsException ex)
            {
                _logger.LogError(ex, $"An error occured creating topic {ex.Results[0].Topic}: {ex.Results[0].Error.Reason}");
                throw;
            }
        }
    }
}
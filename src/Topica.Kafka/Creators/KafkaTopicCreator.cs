using System;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Kafka.Creators
{
    public class KafkaTopicCreator : ITopicCreator
    {
        private readonly IConsumer _consumer;
        private readonly ILogger<KafkaTopicCreator> _logger;

        public KafkaTopicCreator(IConsumer consumer, ILogger<KafkaTopicCreator> logger)
        {
            _consumer = consumer;
            _logger = logger;
        }
        
        public MessagingPlatform MessagingPlatform => MessagingPlatform.Kafka;
        
        public async Task<IConsumer> CreateTopic(ConsumerSettings settings)
        {
            using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = string.Join(",", settings.KafkaBootstrapServers) }).Build();
            
            try
            {
                var meta = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

                if (meta.Topics.Any(x => string.Equals(settings.Source, x.Topic, StringComparison.CurrentCultureIgnoreCase)))
                {
                    _logger.LogInformation($"{nameof(KafkaTopicCreator)}.{nameof(CreateTopic)} topic {settings.Source} already exists!");
                    return _consumer;
                }
                
                await adminClient.CreateTopicsAsync(new[] 
                {
                    new TopicSpecification { Name = settings.Source, ReplicationFactor = 1, NumPartitions = settings.KafkaNumberOfTopicPartitions } 
                });
                
                _logger.LogInformation($"{nameof(KafkaTopicCreator)}.{nameof(CreateTopic)}: Created topic {settings.Source}");

                return _consumer;
            }
            catch (CreateTopicsException ex)
            {
                _logger.LogError(ex, $"{nameof(KafkaTopicCreator)}.{nameof(CreateTopic)}: An error occured creating topic {ex.Results[0].Topic}: {ex.Results[0].Error.Reason}");
                
                throw;
            }
        }
    }
}
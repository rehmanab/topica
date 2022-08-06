using Topica.Topics;

namespace Topica.Kafka.Configuration
{
    public class KafkaTopicConfiguration : TopicConfigurationBase
    {
        public int NumberOfPartitions { get; set; }
    }
}
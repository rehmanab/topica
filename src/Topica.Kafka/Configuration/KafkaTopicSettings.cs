using Topica.Topics;

namespace Topica.Kafka.Configuration
{
    public class KafkaTopicSettings : TopicSettingsBase
    {
        public int NumberOfPartitions { get; set; }
    }
}
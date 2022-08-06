using System;
using System.Threading.Tasks;
using Topica.Contracts;
using Topica.Topics;

namespace Topica.Kafka.Topics
{
    public class KafkaTopicCreator : ITopicCreator
    {
        public MessagingPlatform MessagingPlatform => MessagingPlatform.Kafka;
        
        public Task<string> CreateTopic(TopicConfigurationBase configuration)
        {
            throw new NotImplementedException(configuration.GetType().Name);
        }
    }
}
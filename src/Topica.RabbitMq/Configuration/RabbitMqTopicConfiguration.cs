using System.Collections.Generic;
using Topica.Topics;

namespace Topica.RabbitMq.Configuration
{
    public class RabbitMqTopicConfiguration : TopicConfigurationBase
    {
        public IEnumerable<string> WithSubscribedQueues { get; set; }
    }
}
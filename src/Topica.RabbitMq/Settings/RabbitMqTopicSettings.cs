using System.Collections.Generic;
using Topica.Topics;

namespace Topica.RabbitMq.Settings
{
    public class RabbitMqTopicSettings : TopicSettingsBase
    {
        public IEnumerable<string> WithSubscribedQueues { get; set; }
    }
}
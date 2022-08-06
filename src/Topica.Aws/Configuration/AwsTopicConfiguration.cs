using System.Collections.Generic;
using Topica.Topics;

namespace Topica.Aws.Configuration
{
    public class AwsTopicConfiguration : TopicConfigurationBase
    {
        public IEnumerable<string> WithSubscribedQueues { get; set; } = null!;
        public bool BuildWithErrorQueue { get; set; }
        public int? ErrorQueueMaxReceiveCount { get; set; }
    }
}
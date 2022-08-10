using System.Collections.Generic;
using Topica.Topics;

namespace Topica.Aws.Configuration
{
    public class AwsTopicConfiguration : TopicConfigurationBase
    {
        public IEnumerable<string> WithSubscribedQueues { get; set; } = null!;
        public bool BuildWithErrorQueue { get; set; }
        public int? ErrorQueueMaxReceiveCount { get; set; }
        public int? VisibilityTimeout { get; set; }
        public bool IsFifoQueue { get; set; }
        public bool IsFifoContentBasedDeduplication { get; set; }
        public int? MaximumMessageSize { get; set; }
        public int? MessageRetentionPeriod { get; set; }
        public int? DelaySeconds { get; set; }
        public int? ReceiveMessageWaitTimeSeconds { get; set; }
    }
}
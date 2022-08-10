using Topica.Aws.Configuration;
using Topica.Aws.Queues;

namespace Topica.Aws.Builders
{
    public class SqsConfigurationBuilder : ISqsConfigurationBuilder
    {
        private const int DefaultMaxReceiveCount = 3;

        public QueueConfiguration BuildQueue()
        {
            return BuildQueue(GetDefaultQueueAttributes());
        }

        public QueueConfiguration BuildQueue(AwsQueueAttributes awsQueueAttributes)
        {
            return new QueueConfiguration { QueueAttributes = GetDefaultQueueAttributes(awsQueueAttributes) };
        }

        public QueueConfiguration BuildDefaultQueueWithErrorQueue()
        {
            return BuildQueueWithErrorQueue(DefaultMaxReceiveCount, GetDefaultQueueAttributes());
        }

        public QueueConfiguration BuildDefaultQueueWithErrorQueue(int maxReceiveCount)
        {
            return BuildQueueWithErrorQueue(maxReceiveCount, GetDefaultQueueAttributes());
        }

        public QueueConfiguration BuildQueueWithErrorQueue(int maxReceiveCount, AwsQueueAttributes awsQueueAttributes)
        {
            var config = new QueueConfiguration
            {
                CreateErrorQueue = true,
                MaxReceiveCount = maxReceiveCount,
                QueueAttributes = GetDefaultQueueAttributes(awsQueueAttributes)
            };

            return config;
        }

        public QueueConfiguration BuildWithCreationTypeQueue(QueueCreationType queueCreationType)
        {
            QueueConfiguration configuration;
            switch (queueCreationType)
            {
                case QueueCreationType.SoleQueue:
                    configuration = BuildQueue();
                    break;
                case QueueCreationType.WithErrorQueue:
                    configuration = BuildDefaultQueueWithErrorQueue();
                    break;
                default:
                    return null!;
            }

            return configuration;
        }

        public QueueConfiguration BuildUpdatePolicyQueue(string policy)
        {
            return new QueueConfiguration { QueueAttributes = new AwsQueueAttributes { Policy =  policy} };
        }

        private static AwsQueueAttributes GetDefaultQueueAttributes(AwsQueueAttributes? awsQueueAttributeOverrides = null)
        {
            return new AwsQueueAttributes
            {
                VisibilityTimeout = awsQueueAttributeOverrides?.VisibilityTimeout ?? 30,
                IsFifoQueue = awsQueueAttributeOverrides?.IsFifoQueue ?? false,
                IsFifoContentBasedDeduplication = awsQueueAttributeOverrides?.IsFifoContentBasedDeduplication ?? false,
                MaximumMessageSize = awsQueueAttributeOverrides?.MaximumMessageSize ?? AwsQueueAttributes.MaximumMessageSizeMax,
                MessageRetentionPeriod = awsQueueAttributeOverrides?.MessageRetentionPeriod ?? AwsQueueAttributes.MessageRetentionPeriodMax,
                DelaySeconds = awsQueueAttributeOverrides?.DelaySeconds ?? 0,
                ReceiveMessageWaitTimeSeconds = awsQueueAttributeOverrides?.ReceiveMessageWaitTimeSeconds ?? 0
            };
        }
    }
}
using Topica.Aws.Configuration;
using Topica.Aws.Contracts;
using Topica.Aws.Queues;

namespace Topica.Aws.Builders
{
    public class SqsConfigurationBuilder : ISqsConfigurationBuilder
    {
        private const int DefaultMaxReceiveCount = 3;

        public SqsConfiguration BuildQueue()
        {
            return BuildQueue(GetDefaultQueueAttributes());
        }

        public SqsConfiguration BuildQueue(AwsQueueAttributes awsQueueAttributes)
        {
            return new SqsConfiguration { QueueAttributes = GetDefaultQueueAttributes(awsQueueAttributes) };
        }

        public SqsConfiguration BuildDefaultQueueWithErrorQueue()
        {
            return BuildQueueWithErrorQueue(DefaultMaxReceiveCount, GetDefaultQueueAttributes());
        }

        public SqsConfiguration BuildDefaultQueueWithErrorQueue(int maxReceiveCount)
        {
            return BuildQueueWithErrorQueue(maxReceiveCount, GetDefaultQueueAttributes());
        }

        public SqsConfiguration BuildQueueWithErrorQueue(int maxReceiveCount, AwsQueueAttributes awsQueueAttributes)
        {
            var config = new SqsConfiguration
            {
                CreateErrorQueue = true,
                MaxReceiveCount = maxReceiveCount,
                QueueAttributes = GetDefaultQueueAttributes(awsQueueAttributes)
            };

            return config;
        }

        public SqsConfiguration BuildWithCreationTypeQueue(QueueCreationType queueCreationType)
        {
            SqsConfiguration configuration;
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

        public SqsConfiguration BuildUpdatePolicyQueue(string policy)
        {
            return new SqsConfiguration { QueueAttributes = new AwsQueueAttributes { Policy =  policy} };
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
using Topica.Aws.Contracts;
using Topica.Aws.Queues;

namespace Topica.Aws.Builders
{
    public class AwsSqsConfigurationBuilder : IAwsSqsConfigurationBuilder
    {
        private const int DefaultMaxReceiveCount = 3;

        public AwsSqsConfiguration BuildQueue()
        {
            return BuildQueue(GetDefaultQueueAttributes());
        }

        public AwsSqsConfiguration BuildQueue(AwsQueueAttributes awsQueueAttributes)
        {
            return new AwsSqsConfiguration { QueueAttributes = GetDefaultQueueAttributes(awsQueueAttributes) };
        }

        public AwsSqsConfiguration BuildDefaultQueueWithErrorQueue()
        {
            return BuildQueueWithErrorQueue(DefaultMaxReceiveCount, GetDefaultQueueAttributes());
        }

        public AwsSqsConfiguration BuildDefaultQueueWithErrorQueue(int maxReceiveCount)
        {
            return BuildQueueWithErrorQueue(maxReceiveCount, GetDefaultQueueAttributes());
        }

        public AwsSqsConfiguration BuildQueueWithErrorQueue(int errorQueueMaxReceiveCount, AwsQueueAttributes awsQueueAttributes)
        {
            var config = new AwsSqsConfiguration
            {
                CreateErrorQueue = true,
                ErrorQueueMaxReceiveCount = errorQueueMaxReceiveCount,
                QueueAttributes = GetDefaultQueueAttributes(awsQueueAttributes)
            };

            return config;
        }

        public AwsSqsConfiguration BuildWithCreationTypeQueue(AwsQueueCreationType awsQueueCreationType)
        {
            AwsSqsConfiguration configuration;
            switch (awsQueueCreationType)
            {
                case AwsQueueCreationType.SoleQueue:
                    configuration = BuildQueue();
                    break;
                case AwsQueueCreationType.WithErrorQueue:
                    configuration = BuildDefaultQueueWithErrorQueue();
                    break;
                default:
                    return null!;
            }

            return configuration;
        }

        public AwsSqsConfiguration BuildUpdatePolicyQueue(string policy)
        {
            return new AwsSqsConfiguration { QueueAttributes = new AwsQueueAttributes { Policy =  policy } };
        }

        private static AwsQueueAttributes GetDefaultQueueAttributes(AwsQueueAttributes? awsQueueAttributeOverrides = null)
        {
            return new AwsQueueAttributes
            {
                MessageVisibilityTimeout = awsQueueAttributeOverrides?.MessageVisibilityTimeout ?? 30,
                IsFifoQueue = awsQueueAttributeOverrides?.IsFifoQueue ?? false,
                IsFifoContentBasedDeduplication = awsQueueAttributeOverrides?.IsFifoContentBasedDeduplication ?? false,
                QueueMaximumMessageSize = awsQueueAttributeOverrides?.QueueMaximumMessageSize ?? AwsQueueAttributes.QueueMaximumMessageSizeMax,
                QueueMessageRetentionPeriodSeconds = awsQueueAttributeOverrides?.QueueMessageRetentionPeriodSeconds ?? AwsQueueAttributes.QueueMessageRetentionPeriodMax,
                QueueMessageDelaySeconds = awsQueueAttributeOverrides?.QueueMessageDelaySeconds ?? 0,
                QueueReceiveMessageWaitTimeSeconds = awsQueueAttributeOverrides?.QueueReceiveMessageWaitTimeSeconds ?? 0
            };
        }
    }
}
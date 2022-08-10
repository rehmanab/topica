using Topica.Aws.Configuration;
using Topica.Aws.Queues;
using Topica.Aws.Settings;

namespace Topica.Aws.Builders
{
    public class SqsConfigurationBuilder : ISqsConfigurationBuilder
    {
        private const int DefaultMaxReceiveCount = 3;
        private readonly AwsDefaultAttributeSettings? _awsDefaultAttributeSettings;

        public SqsConfigurationBuilder(AwsDefaultAttributeSettings? awsDefaultAttributeSettings)
        {
            _awsDefaultAttributeSettings = awsDefaultAttributeSettings;
        }

        public QueueConfiguration? BuildDefaultQueue()
        {
            return BuildQueue(GetDefaultQueueAttributes());
        }

        public QueueConfiguration? BuildQueue(AwsQueueAttributes awsQueueAttributes)
        {
            return new QueueConfiguration { QueueAttributes = awsQueueAttributes };
        }

        public QueueConfiguration? BuildDefaultQueueWithErrorQueue()
        {
            return BuildQueueWithErrorQueue(DefaultMaxReceiveCount, GetDefaultQueueAttributes());
        }

        public QueueConfiguration? BuildDefaultQueueWithErrorQueue(int maxReceiveCount)
        {
            return BuildQueueWithErrorQueue(maxReceiveCount, GetDefaultQueueAttributes());
        }

        public QueueConfiguration? BuildQueueWithErrorQueue(int maxReceiveCount, AwsQueueAttributes awsQueueAttributes)
        {
            var config = new QueueConfiguration
            {
                CreateErrorQueue = true,
                MaxReceiveCount = maxReceiveCount,
                QueueAttributes = awsQueueAttributes
            };

            return config;
        }

        public QueueConfiguration? BuildWithCreationTypeQueue(QueueCreationType queueCreationType)
        {
            QueueConfiguration? configuration;
            switch (queueCreationType)
            {
                case QueueCreationType.SoleQueue:
                    configuration = BuildDefaultQueue();
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

        private AwsQueueAttributes GetDefaultQueueAttributes()
        {
            return new AwsQueueAttributes
            {
                VisibilityTimeout = _awsDefaultAttributeSettings?.VisibilityTimeout ?? 30,
                IsFifoQueue = _awsDefaultAttributeSettings?.FifoSettings?.IsFifoQueue ?? true,
                IsFifoContentBasedDeduplication = _awsDefaultAttributeSettings?.FifoSettings?.IsContentBasedDeduplication ?? true,
                MaximumMessageSize = _awsDefaultAttributeSettings?.MaximumMessageSize ?? AwsQueueAttributes.MaximumMessageSizeMax,
                MessageRetentionPeriod = _awsDefaultAttributeSettings?.MessageRetentionPeriod ?? AwsQueueAttributes.MessageRetentionPeriodMax,
                DelaySeconds = _awsDefaultAttributeSettings?.DelayInSeconds ?? 0,
                ReceiveMessageWaitTimeSeconds = _awsDefaultAttributeSettings?.ReceiveMessageWaitTimeSeconds ?? 0
            };
        }
    }
}
using Topica.Aws.Configuration;
using Topica.Aws.Queues;
using Topica.Aws.Settings;

namespace Topica.Aws.Builders
{
    public class SqsConfigurationBuilder : ISqsConfigurationBuilder
    {
        private const int DefaultMaxReceiveCount = 3;
        private readonly AwsDefaultAttributeSettings _awsDefaultAttributeSettings;

        public SqsConfigurationBuilder(AwsDefaultAttributeSettings awsDefaultAttributeSettings)
        {
            _awsDefaultAttributeSettings = awsDefaultAttributeSettings;
        }

        public QueueConfiguration? BuildCreateDefaultQueue()
        {
            return BuildCreateDefaultQueue(GetDefaultQueueAttributes());
        }

        public QueueConfiguration? BuildCreateDefaultQueue(AwsQueueAttributes awsQueueAttributes)
        {
            return new QueueConfiguration { QueueAttributes = awsQueueAttributes };
        }

        public QueueConfiguration? BuildCreateWithErrorQueue()
        {
            return BuildCreateWithErrorQueue(DefaultMaxReceiveCount, GetDefaultQueueAttributes());
        }

        public QueueConfiguration? BuildCreateWithErrorQueue(int maxReceiveCount)
        {
            return BuildCreateWithErrorQueue(maxReceiveCount, GetDefaultQueueAttributes());
        }

        public QueueConfiguration? BuildCreateWithErrorQueue(int maxReceiveCount, AwsQueueAttributes awsQueueAttributes)
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
                    configuration = BuildCreateDefaultQueue();
                    break;
                case QueueCreationType.WithErrorQueue:
                    configuration = BuildCreateWithErrorQueue();
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
            var attributes = new AwsQueueAttributes
            {
                VisibilityTimeout = 30,
                IsFifoQueue = true,
                IsFifoContentBasedDeduplication = true,
                MaximumMessageSize = AwsQueueAttributes.MaximumMessageSizeMax,
                MessageRetentionPeriod = AwsQueueAttributes.MessageRetentionPeriodMax,
                DelaySeconds = 0,
                ReceiveMessageWaitTimeSeconds = 0
            };

            if (_awsDefaultAttributeSettings == null) return attributes;
            
            attributes.DelaySeconds = _awsDefaultAttributeSettings.DelayInSeconds;
            attributes.MaximumMessageSize = _awsDefaultAttributeSettings.MaximumMessageSize;
            attributes.MessageRetentionPeriod = _awsDefaultAttributeSettings.MessageRetentionPeriod;
            attributes.ReceiveMessageWaitTimeSeconds = _awsDefaultAttributeSettings.ReceiveMessageWaitTimeSeconds;
            attributes.VisibilityTimeout = _awsDefaultAttributeSettings.VisibilityTimeout;
            attributes.IsFifoQueue = _awsDefaultAttributeSettings.FifoSettings.IsFifoQueue;
            attributes.IsFifoQueue = _awsDefaultAttributeSettings.FifoSettings.IsContentBasedDeduplication;

            return attributes;
        }
    }
}
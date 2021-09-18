using System;
using Aws.Messaging.Config;
using Aws.Messaging.Contracts;
using Aws.Messaging.Queue.SQS;
using Aws.Messaging.Settings;

namespace Aws.Messaging.Builders
{
    public class SqsConfigurationBuilder : ISqsConfigurationBuilder
    {
        private const int DefaultMaxReceiveCount = 3;
        private readonly AwsDefaultAttributeSettings _awsDefaultAttributeSettings;

        public SqsConfigurationBuilder(AwsDefaultAttributeSettings awsDefaultAttributeSettings)
        {
            _awsDefaultAttributeSettings = awsDefaultAttributeSettings;
        }

        public SqsConfiguration BuildCreateDefaultQueue()
        {
            return BuildCreateDefaultQueue(GetDefaultQueueAttributes());
        }

        public SqsConfiguration BuildCreateDefaultQueue(AwsQueueAttributes awsQueueAttributes)
        {
            return new SqsConfiguration { QueueAttributes = awsQueueAttributes };
        }

        public SqsConfiguration BuildCreateWithErrorQueue()
        {
            return BuildCreateWithErrorQueue(DefaultMaxReceiveCount, GetDefaultQueueAttributes());
        }

        public SqsConfiguration BuildCreateWithErrorQueue(int maxReceiveCount)
        {
            return BuildCreateWithErrorQueue(maxReceiveCount, GetDefaultQueueAttributes());
        }

        public SqsConfiguration BuildCreateWithErrorQueue(int maxReceiveCount, AwsQueueAttributes awsQueueAttributes)
        {
            var config = new SqsConfiguration
            {
                CreateErrorQueue = true,
                MaxReceiveCount = maxReceiveCount,
                QueueAttributes = awsQueueAttributes
            };

            return config;
        }

        public SqsConfiguration BuildWithCreationTypeQueue(QueueCreationType queueCreationType)
        {
            SqsConfiguration configuration;
            switch (queueCreationType)
            {
                case QueueCreationType.SoleQueue:
                    configuration = BuildCreateDefaultQueue();
                    break;
                case QueueCreationType.WithErrorQueue:
                    configuration = BuildCreateWithErrorQueue();
                    break;
                default:
                    return null;
            }

            return configuration;
        }

        public SqsConfiguration BuildUpdatePolicyQueue(string policy)
        {
            return new SqsConfiguration { QueueAttributes = new AwsQueueAttributes { Policy =  policy} };
        }

        private AwsQueueAttributes GetDefaultQueueAttributes()
        {
            var attributes = new AwsQueueAttributes();

            if (_awsDefaultAttributeSettings == null) return attributes;
            
            attributes.DelaySeconds = _awsDefaultAttributeSettings.DelayInSeconds;
            attributes.MaximumMessageSize = _awsDefaultAttributeSettings.MaximumMessageSize;
            attributes.MessageRetentionPeriod = _awsDefaultAttributeSettings.MessageRetentionPeriod;
            attributes.ReceiveMessageWaitTimeSeconds = _awsDefaultAttributeSettings.ReceiveMessageWaitTimeSeconds;
            attributes.VisibilityTimeout = _awsDefaultAttributeSettings.VisibilityTimeout;

            return attributes;
        }
    }
}
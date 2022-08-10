using System;
using System.Threading.Tasks;
using Topica.Aws.Configuration;
using Topica.Aws.Queues;
using Topica.Contracts;
using Topica.Topics;

namespace Topica.Aws.Topics
{
    public class AwsTopicCreator : ITopicCreator
    {
        private const int ErrorQueueMaxReceiveCountDefault = 5;
        private readonly IAwsTopicBuilder _awsTopicBuilder;
        private readonly ISqsConfigurationBuilder _sqsConfigurationBuilder;
        
        public MessagingPlatform MessagingPlatform => MessagingPlatform.Aws;

        public AwsTopicCreator(IAwsTopicBuilder awsTopicBuilder, ISqsConfigurationBuilder sqsConfigurationBuilder)
        {
            _awsTopicBuilder = awsTopicBuilder;
            _sqsConfigurationBuilder = sqsConfigurationBuilder;
        }

        public async Task<string> CreateTopic(TopicConfigurationBase configuration)
        {
            var config = configuration as AwsTopicConfiguration;

            if (config == null)
            {
                throw new Exception($"{nameof(AwsTopicCreator)}.{nameof(CreateTopic)} needs an {nameof(AwsTopicConfiguration)} ");
            }
            
            var creator = _awsTopicBuilder.WithTopicName(configuration.TopicName);
            foreach (var subscribedQueueName in config.WithSubscribedQueues)
            {
                creator = creator.WithSubscribedQueue(subscribedQueueName);
            }

            var awsQueueAttributes = new AwsQueueAttributes
            {
                VisibilityTimeout = config.VisibilityTimeout,
                IsFifoQueue = config.IsFifoQueue,
                IsFifoContentBasedDeduplication = config.IsFifoContentBasedDeduplication,
                MaximumMessageSize = config.MaximumMessageSize,
                MessageRetentionPeriod = config.MessageRetentionPeriod,
                DelaySeconds = config.DelaySeconds,
                ReceiveMessageWaitTimeSeconds = config.ReceiveMessageWaitTimeSeconds
            };
            
            var queueConfiguration = _sqsConfigurationBuilder.BuildQueue(awsQueueAttributes);
            
            if (config.BuildWithErrorQueue)
            {
                queueConfiguration.CreateErrorQueue = true;
                queueConfiguration.MaxReceiveCount = config.ErrorQueueMaxReceiveCount ?? ErrorQueueMaxReceiveCountDefault;
            }
            
            return await creator.WithQueueConfiguration(queueConfiguration).BuildAsync();
        }
    }
}
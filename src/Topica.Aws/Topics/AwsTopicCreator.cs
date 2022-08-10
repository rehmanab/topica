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
            
            var queueConfiguration = _sqsConfigurationBuilder.BuildDefaultQueue();
            
            if (config.BuildWithErrorQueue)
            {
                queueConfiguration = _sqsConfigurationBuilder.BuildDefaultQueueWithErrorQueue(config.ErrorQueueMaxReceiveCount ?? 5);
            }
            
            return await creator.WithQueueConfiguration(queueConfiguration).BuildAsync();
        }
    }
}
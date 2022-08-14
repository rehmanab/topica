using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly IConsumer _consumer;
        private readonly ILogger<AwsTopicCreator> _logger;

        public MessagingPlatform MessagingPlatform => MessagingPlatform.Aws;

        public AwsTopicCreator(IAwsTopicBuilder awsTopicBuilder,
            ISqsConfigurationBuilder sqsConfigurationBuilder,
            IConsumer consumer,
            ILogger<AwsTopicCreator> logger)
        {
            _awsTopicBuilder = awsTopicBuilder;
            _sqsConfigurationBuilder = sqsConfigurationBuilder;
            _consumer = consumer;
            _logger = logger;
        }

        public async Task<IConsumer> CreateTopic(TopicSettingsBase settings)
        {
            var config = settings as AwsTopicSettings;

            if (config == null)
            {
                throw new Exception($"{nameof(AwsTopicCreator)}.{nameof(CreateTopic)} needs an {nameof(AwsTopicSettings)} ");
            }
            
            var creator = _awsTopicBuilder.WithTopicName(settings.TopicName);
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

            var topic = await creator.WithQueueConfiguration(queueConfiguration).BuildAsync();
            
            _logger.LogInformation($"{nameof(AwsTopicCreator)}.{nameof(CreateTopic)}: Created topic {topic}");

            return _consumer;
        }
    }
}
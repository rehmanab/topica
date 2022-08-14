using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Aws.Configuration;
using Topica.Aws.Queues;
using Topica.Contracts;
using Topica.Settings;

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

        public async Task<IConsumer> CreateTopic(ConsumerSettings settings)
        {
            var creator = _awsTopicBuilder.WithTopicName(settings.Source);
            foreach (var subscribedQueueName in settings.WithSubscribedQueues)
            {
                creator = creator.WithSubscribedQueue(subscribedQueueName);
            }

            var awsQueueAttributes = new AwsQueueAttributes
            {
                VisibilityTimeout = settings.AwsVisibilityTimeout,
                IsFifoQueue = settings.AwsIsFifoQueue,
                IsFifoContentBasedDeduplication = settings.AwsIsFifoContentBasedDeduplication,
                MaximumMessageSize = settings.AwsMaximumMessageSize,
                MessageRetentionPeriod = settings.AwsMessageRetentionPeriod,
                DelaySeconds = settings.AwsDelaySeconds,
                ReceiveMessageWaitTimeSeconds = settings.AwsReceiveMessageWaitTimeSeconds
            };
            
            var queueConfiguration = _sqsConfigurationBuilder.BuildQueue(awsQueueAttributes);
            
            if (settings.AwsBuildWithErrorQueue)
            {
                queueConfiguration.CreateErrorQueue = true;
                queueConfiguration.MaxReceiveCount = settings.AwsErrorQueueMaxReceiveCount ?? ErrorQueueMaxReceiveCountDefault;
            }

            var topic = await creator.WithQueueConfiguration(queueConfiguration).BuildAsync();
            
            _logger.LogInformation($"{nameof(AwsTopicCreator)}.{nameof(CreateTopic)}: Created topic {topic}");

            return _consumer;
        }
    }
}
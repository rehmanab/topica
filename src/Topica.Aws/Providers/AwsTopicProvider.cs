using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Aws.Contracts;
using Topica.Aws.Queues;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Aws.Providers
{
    public class AwsTopicProvider : ITopicProvider
    {
        private const int ErrorQueueMaxReceiveCountDefault = 5;
        private readonly IAwsTopicBuilder _awsTopicBuilder;
        private readonly ISqsConfigurationBuilder _sqsConfigurationBuilder;
        private readonly IConsumer _consumer;
        private readonly IProducerBuilder _producerBuilder;
        private readonly ILogger<AwsTopicProvider> _logger;

        public MessagingPlatform MessagingPlatform => MessagingPlatform.Aws;

        public AwsTopicProvider(IAwsTopicBuilder awsTopicBuilder,
            ISqsConfigurationBuilder sqsConfigurationBuilder,
            IConsumer consumer,
            IProducerBuilder producerBuilder,
            ILogger<AwsTopicProvider> logger)
        {
            _awsTopicBuilder = awsTopicBuilder;
            _sqsConfigurationBuilder = sqsConfigurationBuilder;
            _consumer = consumer;
            _producerBuilder = producerBuilder;
            _logger = logger;
        }

        public async Task<IConsumer> CreateTopicAsync(ConsumerSettings settings)
        {
            await CreateTopicAsync(settings.Source, settings.WithSubscribedQueues, settings.AwsVisibilityTimeout,
                settings.AwsIsFifoQueue, settings.AwsIsFifoContentBasedDeduplication, settings.AwsMaximumMessageSize,
                settings.AwsMessageRetentionPeriod, settings.AwsDelaySeconds, settings.AwsReceiveMessageWaitTimeSeconds,
                settings.AwsBuildWithErrorQueue, settings.AwsErrorQueueMaxReceiveCount);
            
            return _consumer;
        }

        public async Task<IProducerBuilder> CreateTopicAsync(ProducerSettings settings)
        {
            await CreateTopicAsync(settings.Source, settings.WithSubscribedQueues, settings.AwsVisibilityTimeout,
                settings.AwsIsFifoQueue, settings.AwsIsFifoContentBasedDeduplication, settings.AwsMaximumMessageSize,
                settings.AwsMessageRetentionPeriod, settings.AwsDelaySeconds, settings.AwsReceiveMessageWaitTimeSeconds,
                settings.AwsBuildWithErrorQueue, settings.AwsErrorQueueMaxReceiveCount);
            
            return _producerBuilder;
        }

        public IConsumer GetConsumer()
        {
            return _consumer;
        }

        public IProducerBuilder GetProducerBuilder()
        {
            return _producerBuilder;
        }

        private async Task CreateTopicAsync(string source, string[] withSubscribedQueues, int? awsVisibilityTimeout,
            bool awsIsFifoQueue, bool awsIsFifoContentBasedDeduplication, int? awsMaximumMessageSize,
            int? awsMessageRetentionPeriod, int? awsDelaySeconds, int? awsReceiveMessageWaitTimeSeconds,
            bool awsBuildWithErrorQueue, int? awsErrorQueueMaxReceiveCount)
        {
            var creator = _awsTopicBuilder.WithTopicName(source);
            foreach (var subscribedQueueName in withSubscribedQueues)
            {
                creator = creator.WithSubscribedQueue(subscribedQueueName);
            }

            var awsQueueAttributes = new AwsQueueAttributes
            {
                VisibilityTimeout = awsVisibilityTimeout,
                IsFifoQueue = awsIsFifoQueue,
                IsFifoContentBasedDeduplication = awsIsFifoContentBasedDeduplication,
                MaximumMessageSize = awsMaximumMessageSize,
                MessageRetentionPeriod = awsMessageRetentionPeriod,
                DelaySeconds = awsDelaySeconds,
                ReceiveMessageWaitTimeSeconds = awsReceiveMessageWaitTimeSeconds
            };
            
            var queueConfiguration = _sqsConfigurationBuilder.BuildQueue(awsQueueAttributes);
            
            if (awsBuildWithErrorQueue)
            {
                queueConfiguration.CreateErrorQueue = true;
                queueConfiguration.MaxReceiveCount = awsErrorQueueMaxReceiveCount ?? ErrorQueueMaxReceiveCountDefault;
            }

            var topic = await creator.WithQueueConfiguration(queueConfiguration).BuildAsync();
            
            _logger.LogInformation($"{nameof(AwsTopicProvider)}.{nameof(CreateTopicAsync)}: Created topic {topic}");
        }
    }
}
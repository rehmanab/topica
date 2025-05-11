using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Aws.Contracts;
using Topica.Aws.Queues;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Aws.Providers
{
    public class AwsTopicProvider(
        IAwsTopicBuilder awsTopicBuilder,
        ISqsConfigurationBuilder sqsConfigurationBuilder,
        ILogger<AwsTopicProvider> logger)
        : ITopicProvider
    {
        private const int ErrorQueueMaxReceiveCountDefault = 5;

        public MessagingPlatform MessagingPlatform => MessagingPlatform.Aws;

        public async Task CreateTopicAsync(ConsumerSettings settings)
        {
            await CreateTopicAsync(settings.Source, settings.WithSubscribedQueues, settings.AwsVisibilityTimeout,
                settings.AwsIsFifoQueue, settings.AwsIsFifoContentBasedDeduplication, settings.AwsMaximumMessageSize,
                settings.AwsMessageRetentionPeriod, settings.AwsDelaySeconds, settings.AwsReceiveMessageWaitTimeSeconds,
                settings.AwsBuildWithErrorQueue, settings.AwsErrorQueueMaxReceiveCount);
        }

        public async Task CreateTopicAsync(ProducerSettings settings)
        {
            await CreateTopicAsync(settings.Source, settings.WithSubscribedQueues, settings.AwsVisibilityTimeout,
                settings.AwsIsFifoQueue, settings.AwsIsFifoContentBasedDeduplication, settings.AwsMaximumMessageSize,
                settings.AwsMessageRetentionPeriod, settings.AwsDelaySeconds, settings.AwsReceiveMessageWaitTimeSeconds,
                settings.AwsBuildWithErrorQueue, settings.AwsErrorQueueMaxReceiveCount);
        }

        private async Task CreateTopicAsync(string source, string[] withSubscribedQueues, int? awsVisibilityTimeout,
            bool awsIsFifoQueue, bool awsIsFifoContentBasedDeduplication, int? awsMaximumMessageSize,
            int? awsMessageRetentionPeriod, int? awsDelaySeconds, int? awsReceiveMessageWaitTimeSeconds,
            bool awsBuildWithErrorQueue, int? awsErrorQueueMaxReceiveCount)
        {
            var creator = awsTopicBuilder.WithTopicName(source);
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
            
            var sqsConfiguration = sqsConfigurationBuilder.BuildQueue(awsQueueAttributes);
            
            if (awsBuildWithErrorQueue)
            {
                sqsConfiguration.CreateErrorQueue = true;
                sqsConfiguration.MaxReceiveCount = awsErrorQueueMaxReceiveCount ?? ErrorQueueMaxReceiveCountDefault;
            }

            var topic = await creator.WithSqsConfiguration(sqsConfiguration).BuildAsync();
            
            logger.LogInformation("{AwsTopicProviderName}.{CreateTopicAsyncName}: Created topic {Topic}", nameof(AwsTopicProvider), nameof(CreateTopicAsync), topic);
        }
    }
}
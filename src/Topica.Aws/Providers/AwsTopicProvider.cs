using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Microsoft.Extensions.Logging;
using Topica.Aws.Consumers;
using Topica.Aws.Contracts;
using Topica.Aws.Producer;
using Topica.Aws.Queues;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Aws.Providers
{
    public class AwsTopicProvider(
        IAwsProviderTopicBuilder awsProviderTopicBuilder,
        IAwsSqsConfigurationBuilder awsSqsConfigurationBuilder,
        IAmazonSimpleNotificationService snsClient,
        IAmazonSQS sqsClient,
        IMessageHandlerExecutor messageHandlerExecutor,
        IAwsQueueService awsQueueService,
        ILogger<AwsTopicProvider> logger) : ITopicProvider
    {
        public MessagingPlatform MessagingPlatform => MessagingPlatform.Aws;

        public async Task CreateTopicAsync(MessagingSettings settings)
        {
            var creator = awsProviderTopicBuilder.WithTopicName(settings.Source);
            foreach (var subscribedQueueName in settings.AwsWithSubscribedQueues)
            {
                creator = creator.WithSubscribedQueue(subscribedQueueName);
            }

            var awsQueueAttributes = new AwsQueueAttributes
            {
                MessageVisibilityTimeout = settings.AwsMessageVisibilityTimeoutSeconds,
                IsFifoQueue = settings.AwsIsFifoQueue,
                IsFifoContentBasedDeduplication = settings.AwsIsFifoContentBasedDeduplication,
                QueueMaximumMessageSize = settings.AwsQueueMaximumMessageSize,
                QueueMessageRetentionPeriodSeconds = settings.AwsQueueMessageRetentionPeriodSeconds,
                QueueMessageDelaySeconds = settings.AwsQueueMessageDelaySeconds,
                QueueReceiveMessageWaitTimeSeconds = settings.AwsQueueReceiveMessageWaitTimeSeconds
            };
            
            var sqsConfiguration = awsSqsConfigurationBuilder.BuildQueue(awsQueueAttributes);
            
            if (settings.AwsBuildWithErrorQueue)
            {
                sqsConfiguration.CreateErrorQueue = true;
                sqsConfiguration.ErrorQueueMaxReceiveCount = settings.AwsErrorQueueMaxReceiveCount;
            }

            var topic = await creator.WithSqsConfiguration(sqsConfiguration).BuildAsync();
            
            logger.LogInformation("{AwsTopicProviderName}.{CreateTopicAsyncName}: Created topic {Topic}", nameof(AwsTopicProvider), nameof(CreateTopicAsync), topic);
        }

        public async Task<IConsumer> ProvideConsumerAsync(string consumerName, MessagingSettings messagingSettings)
        {
            await Task.CompletedTask;
            
            return new AwsQueueConsumer(sqsClient, messageHandlerExecutor, awsQueueService, messagingSettings, logger);
        }

        public async Task<IProducer> ProvideProducerAsync(string producerName, MessagingSettings messagingSettings)
        {
            await Task.CompletedTask;
            
            return new AwsTopicProducer(producerName, snsClient);
        }
    }
}
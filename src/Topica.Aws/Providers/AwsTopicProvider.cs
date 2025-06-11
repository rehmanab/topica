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
        IPollyRetryService pollyRetryService,
        IAmazonSimpleNotificationService snsClient,
        IAmazonSQS sqsClient,
        IMessageHandlerExecutor messageHandlerExecutor,
        IAwsTopicService awsTopicService,
        IAwsQueueService awsQueueService,
        ILogger<AwsTopicProvider> logger) : ITopicProvider
    {
        public MessagingPlatform MessagingPlatform => MessagingPlatform.Aws;

        public async Task CreateTopicAsync(MessagingSettings settings)
        {
            _ = await awsTopicService.CreateTopicWithOptionalQueuesSubscribedAsync(settings.Source, settings.AwsWithSubscribedQueues, new AwsSqsConfiguration
            {
                QueueAttributes = new AwsQueueAttributes
                {
                    MessageVisibilityTimeout = settings.AwsMessageVisibilityTimeoutSeconds,
                    IsFifoQueue = settings.AwsIsFifoQueue,
                    IsFifoContentBasedDeduplication = settings.AwsIsFifoContentBasedDeduplication,
                    QueueMaximumMessageSize = settings.AwsQueueMaximumMessageSizeKb,
                    QueueMessageRetentionPeriodSeconds = settings.AwsQueueMessageRetentionPeriodSeconds,
                    QueueMessageDelaySeconds = settings.AwsQueueMessageDelaySeconds,
                    QueueReceiveMessageWaitTimeSeconds = settings.AwsQueueReceiveMessageWaitTimeSeconds
                },
                CreateErrorQueue = settings.AwsBuildWithErrorQueue,
                ErrorQueueMaxReceiveCount = settings.AwsErrorQueueMaxReceiveCount
            });
        }

        public async Task<IConsumer> ProvideConsumerAsync(MessagingSettings messagingSettings)
        {
            await Task.CompletedTask;

            return new AwsQueueConsumer(pollyRetryService, sqsClient, messageHandlerExecutor, awsQueueService, messagingSettings, logger);
        }

        public async Task<IProducer> ProvideProducerAsync(string producerName, MessagingSettings messagingSettings)
        {
            await Task.CompletedTask;

            return new AwsTopicProducer(producerName, awsTopicService, snsClient, messagingSettings.AwsIsFifoQueue);
        }
    }
}
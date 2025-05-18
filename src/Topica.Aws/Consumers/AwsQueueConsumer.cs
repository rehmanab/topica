using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Topica.Aws.Contracts;
using Topica.Aws.Messages;
using Topica.Contracts;
using Topica.Messages;
using Topica.Settings;

namespace Topica.Aws.Consumers
{
    public class AwsQueueConsumer : IConsumer
    {
        private readonly ITopicProviderFactory _topicProviderFactory;
        private readonly IAmazonSQS _client;
        private readonly IMessageHandlerExecutor _messageHandlerExecutor;
        private readonly IAwsQueueService _awsQueueService;
        private readonly ResiliencePipeline _retryPipeline;
        private readonly ILogger<AwsQueueConsumer> _logger;

        public AwsQueueConsumer(ITopicProviderFactory topicProviderFactory, IAmazonSQS client, IMessageHandlerExecutor messageHandlerExecutor, IAwsQueueService awsQueueService, ILogger<AwsQueueConsumer> logger)
        {
            _topicProviderFactory = topicProviderFactory;
            _client = client;
            _messageHandlerExecutor = messageHandlerExecutor;
            _awsQueueService = awsQueueService;
            _retryPipeline = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Constant,
                Delay = TimeSpan.FromSeconds(5),
                MaxRetryAttempts = int.MaxValue,
                OnRetry = args =>
                {
                    logger.LogWarning("Retrying: {ArgsAttemptNumber} in {RetryDelayTotalSeconds} seconds", args.AttemptNumber + 1, args.RetryDelay.TotalSeconds);
                    return default;
                }
            }).Build();
            _logger = logger;
        }

        public async Task ConsumeAsync<T>(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken) where T : IHandler
        {
            await _topicProviderFactory.Create(MessagingPlatform.Aws).CreateTopicAsync(consumerSettings);

            Parallel.ForEach(Enumerable.Range(1, consumerSettings.NumberOfInstances), index =>
            {
                _retryPipeline.ExecuteAsync(x => StartAsync<T>($"{consumerName}-consumer-({index})", consumerSettings, x), cancellationToken);
            });
        }

        private async ValueTask StartAsync<T>(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken) where T: IHandler
        {
            try
            {
                var queueUrl = await _awsQueueService.GetQueueUrlAsync(consumerSettings.SubscribeToSource);

                if (string.IsNullOrWhiteSpace(queueUrl))
                {
                    _logger.LogError("{AwsQueueConsumerName}: queue: {ConsumerSettingsSubscribeToSource} does not exist", nameof(AwsQueueConsumer), consumerSettings.SubscribeToSource);
                    throw new ApplicationException($"{nameof(AwsQueueConsumer)}: queue: {consumerSettings.SubscribeToSource} does not exist.");
                }

                _logger.LogInformation("{AwsQueueConsumerName}:: {ConsumerName} started on Queue: {QueueUrl}", nameof(AwsQueueConsumer), consumerName, queueUrl);

                var receiveMessageRequest = new ReceiveMessageRequest { QueueUrl = queueUrl, MaxNumberOfMessages = consumerSettings.AwsReceiveMaximumNumberOfMessages};

                while (!cancellationToken.IsCancellationRequested)
                {
                    var receiveMessageResponse = await _client.ReceiveMessageAsync(receiveMessageRequest, cancellationToken);

                    if (receiveMessageResponse?.Messages == null)
                    {
                        await Task.Delay(1000, cancellationToken);
                        continue;
                    }
                    
                    foreach (var message in receiveMessageResponse.Messages.OfType<Message>())
                    {
                        // _logger.LogDebug("SQS: Original Message from AWS: {SerializeObject}", JsonConvert.SerializeObject(message));
                        
                        var baseMessage = BaseMessage.Parse<BaseMessage>(message.Body);
                        var messageBody = message.Body;

                        if (baseMessage == null)
                        {
                            _logger.LogWarning("SQS: message body could not be serialized into BaseMessage ({MessageId}): {MessageBody}", message.MessageId, message.Body);
                            continue;
                        }
                        
                        // SNS notification sent, our message will be the Message property
                        if (baseMessage.Type == "Notification")
                        {
                            var notification = AwsNotification.Parse(messageBody);	
                            if (notification == null)
                            {
                                _logger.LogError("SQS: Error: could not convert Notification to AwsNotification object");
                                continue;
                            }
                            // baseMessage = BaseMessage.Parse<BaseMessage>(notification.Message);
                            messageBody = notification.Message;
                        }

                        var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync<T>(messageBody);
                        // _logger.LogDebug("**** {AwsQueueConsumerName}: {ConsumerName}: {HandlerName} {Succeeded} @ {DateTime} ****", nameof(AwsQueueConsumer), consumerName, handlerName, success ? "SUCCEEDED" : "FAILED", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        
                        if (!success) continue;

                        if (!await _awsQueueService.DeleteMessageAsync(queueUrl, message.ReceiptHandle))
                        {
                            _logger.LogError("{AwsQueueConsumerName}: {ConsumerName}: could not delete message on Queue: {ConsumerSettingsSubscribeToSource}", nameof(AwsQueueConsumer), consumerName, consumerSettings.SubscribeToSource);
                        }
                    }

                    await Task.Delay(250, cancellationToken);
                }

                _logger.LogInformation("{AwsQueueConsumerName}: {ConsumerName}: Stopped Queue: {ConsumerSettingsSubscribeToSource}", nameof(AwsQueueConsumer), consumerName, consumerSettings.SubscribeToSource);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("{AwsQueueConsumerName}: {ConsumerName}: Stopped Queue: {ConsumerSettingsSubscribeToSource}", nameof(AwsQueueConsumer), consumerName, consumerSettings.SubscribeToSource);
                _client.Dispose();
            }
            catch (AggregateException ex)
            {
                foreach (var inner in ex.Flatten().InnerExceptions)
                {
                    _logger.LogError(inner, "{AwsQueueConsumerName}: {ConsumerName}: AggregateException:", nameof(AwsQueueConsumer), consumerName);
                }

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{AwsQueueConsumerName}: {ConsumerName}: Exception:", nameof(AwsQueueConsumer), consumerName);
                throw;
            }
        }
    }
}
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
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
        private readonly IAmazonSQS _client;
        private readonly IMessageHandlerExecutor _messageHandlerExecutor;
        private readonly IAwsQueueService _awsQueueService;
        private readonly MessagingSettings _messagingSettings;
        private readonly ResiliencePipeline _retryPipeline;
        private readonly ILogger _logger;

        public AwsQueueConsumer(IAmazonSQS client, IMessageHandlerExecutor messageHandlerExecutor, IAwsQueueService awsQueueService, MessagingSettings messagingSettings, ILogger logger)
        {
            _client = client;
            _messageHandlerExecutor = messageHandlerExecutor;
            _awsQueueService = awsQueueService;
            _messagingSettings = messagingSettings;
            _retryPipeline = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Constant,
                Delay = TimeSpan.FromSeconds(5),
                MaxRetryAttempts = int.MaxValue,
                OnRetry = args =>
                {
                    logger.LogWarning("Retrying until connected and/or Topic/Queue exists: retry attempt: {ArgsAttemptNumber} - Retry in {RetryDelayTotalSeconds} seconds", args.AttemptNumber + 1, args.RetryDelay.TotalSeconds);
                    return default;
                }
            }).Build();
            _logger = logger;
        }

        public async Task ConsumeAsync(CancellationToken cancellationToken)
        {
            Parallel.ForEach(Enumerable.Range(1, _messagingSettings.NumberOfInstances), index =>
            {
                _retryPipeline.ExecuteAsync(x => StartAsync($"{_messagingSettings.WorkerName}-({index})", _messagingSettings, x), cancellationToken);
            });

            await Task.CompletedTask;
        }

        private async ValueTask StartAsync(string consumerName, MessagingSettings messagingSettings, CancellationToken cancellationToken)
        {
            try
            {
                var queueUrl = await _awsQueueService.GetQueueUrlAsync(messagingSettings.AwsIsFifoQueue && !messagingSettings.SubscribeToSource.EndsWith(".fifo") ? $"{messagingSettings.SubscribeToSource}.fifo" : messagingSettings.SubscribeToSource);

                if (string.IsNullOrWhiteSpace(queueUrl))
                {
                    _logger.LogError("{AwsQueueConsumerName}: queue: {ConsumerSettingsSubscribeToSource} does not exist", nameof(AwsQueueConsumer), messagingSettings.SubscribeToSource);
                    throw new ApplicationException($"{nameof(AwsQueueConsumer)}: queue: {messagingSettings.SubscribeToSource} does not exist.");
                }

                _logger.LogInformation("{AwsQueueConsumerName}:: {ConsumerName} started on Queue: {QueueUrl}", nameof(AwsQueueConsumer), consumerName, queueUrl);

                var receiveMessageRequest = new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl, 
                    MaxNumberOfMessages = messagingSettings.AwsQueueReceiveMaximumNumberOfMessages, 
                    VisibilityTimeout = messagingSettings.AwsMessageVisibilityTimeoutSeconds, 
                    WaitTimeSeconds = messagingSettings.AwsQueueReceiveMessageWaitTimeSeconds
                };

                while (!cancellationToken.IsCancellationRequested)
                {
                    var receiveMessageResponse = await _client.ReceiveMessageAsync(receiveMessageRequest, cancellationToken);

                    if (receiveMessageResponse?.Messages == null)
                    {
                        await Task.Delay(50, cancellationToken);
                        continue;
                    }
                    
                    foreach (var message in receiveMessageResponse.Messages.OfType<Message>())
                    {
                        // _logger.LogDebug("SQS: Original Message from AWS: {SerializeObject}", JsonConvert.SerializeObject(message));
                        
                        var baseMessage = BaseMessage.Parse<BaseMessage>(message.Body);
                        if (baseMessage == null)
                        {
                            _logger.LogWarning("SQS: message body could not be serialized into BaseMessage ({MessageId}): {MessageBody}", message.MessageId, message.Body);
                            continue;
                        }
                        
                        var messageBody = message.Body;
                        if (string.IsNullOrWhiteSpace(messageBody))
                        {
                            _logger.LogWarning("SQS: message body is empty or null ({MessageId})", message.MessageId);
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
                            if (string.IsNullOrWhiteSpace(messageBody))
                            {
                                _logger.LogWarning("SQS: message body for the notification (SNS message) is empty or null ({MessageId})", message.MessageId);
                                continue;
                            }
                        }

                        var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync(messageBody);
                        _logger.LogDebug("**** {AwsQueueConsumerName}: {ConsumerName}: {HandlerName} {Succeeded} ****", nameof(AwsQueueConsumer), consumerName, handlerName, success ? "SUCCEEDED" : "FAILED");
                        
                        if (!success) continue; // Undeleted messages will be retried by AWS SQS, or sent to the error queue if configured

                        if (!await _awsQueueService.DeleteMessageAsync(queueUrl, message.ReceiptHandle))
                        {
                            _logger.LogError("{AwsQueueConsumerName}: {ConsumerName}: could not delete message on Queue: {ConsumerSettingsSubscribeToSource}", nameof(AwsQueueConsumer), consumerName, messagingSettings.SubscribeToSource);
                        }
                    }
                }

                _logger.LogInformation("{AwsQueueConsumerName}: {ConsumerName}: Stopped Queue: {ConsumerSettingsSubscribeToSource}", nameof(AwsQueueConsumer), consumerName, messagingSettings.SubscribeToSource);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("{AwsQueueConsumerName}: {ConsumerName}: Stopped Queue: {ConsumerSettingsSubscribeToSource}", nameof(AwsQueueConsumer), consumerName, messagingSettings.SubscribeToSource);
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
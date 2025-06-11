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
        private readonly IPollyRetryService _pollyRetryService;
        private readonly IAmazonSQS? _sqsClient;
        private readonly IMessageHandlerExecutor _messageHandlerExecutor;
        private readonly IAwsQueueService _awsQueueService;
        private readonly MessagingSettings _messagingSettings;
        private readonly ResiliencePipeline _retryPipeline;
        private readonly ILogger _logger;

        public AwsQueueConsumer(IPollyRetryService pollyRetryService, IAmazonSQS sqsClient, IMessageHandlerExecutor messageHandlerExecutor, IAwsQueueService awsQueueService, MessagingSettings messagingSettings, ILogger logger)
        {
            _pollyRetryService = pollyRetryService;
            _sqsClient = sqsClient;
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
                    logger.LogWarning("**** {Name}:{ConsumerName}: Retrying until connected and/or Topic/Queue exists: retry attempt: {ArgsAttemptNumber} - Retry in {RetryDelayTotalSeconds} seconds", nameof(AwsQueueConsumer), _messagingSettings.WorkerName, args.AttemptNumber + 1, args.RetryDelay.TotalSeconds);
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

        public ValueTask DisposeAsync()
        {
            _sqsClient?.Dispose();
            return ValueTask.CompletedTask;
        }

        private async ValueTask StartAsync(string consumerName, MessagingSettings messagingSettings, CancellationToken cancellationToken)
        {
            try
            {
                var queueUrl = await _pollyRetryService.WaitAndRetryAsync<Exception, string?>
                (
                    5,
                    _ => TimeSpan.FromSeconds(3),
                    (delegateResult, ts, index, context) => _logger.LogWarning("**** RETRY: {Name}:{ConsumerName}:  Retry attempt: {RetryAttempt} - Retry in {RetryDelayTotalSeconds} - Result: {Result}", nameof(AwsQueueConsumer), consumerName, index, ts, delegateResult.Exception?.Message ?? "The result did not pass the result condition."),
                    result => string.IsNullOrWhiteSpace(result) || !result.StartsWith("http"),
                    () => _awsQueueService.GetQueueUrlAsync(messagingSettings.SubscribeToSource, messagingSettings.AwsIsFifoQueue, cancellationToken),
                    false
                );

                if (string.IsNullOrWhiteSpace(queueUrl))
                {
                    _logger.LogError("{Name}: queue: {ConsumerSettingsSubscribeToSource} does not exist", nameof(AwsQueueConsumer), messagingSettings.SubscribeToSource);
                    throw new ApplicationException($"{nameof(AwsQueueConsumer)}: queue: {messagingSettings.SubscribeToSource} does not exist.");
                }

                _logger.LogInformation("**** CONSUMER STARTED: {ConsumerName} consuming from QueueUrl: {QueueUrl}", consumerName, queueUrl);

                var receiveMessageRequest = new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl, 
                    MaxNumberOfMessages = messagingSettings.AwsQueueReceiveMaximumNumberOfMessages, 
                    VisibilityTimeout = messagingSettings.AwsMessageVisibilityTimeoutSeconds, 
                    WaitTimeSeconds = messagingSettings.AwsQueueReceiveMessageWaitTimeSeconds
                };

                while (!cancellationToken.IsCancellationRequested)
                {
                    if(_sqsClient == null)
                    {
                        _logger.LogError("{Name}: {ConsumerName}: SQS client is not initialized", nameof(AwsQueueConsumer), consumerName);
                        throw new InvalidOperationException("SQS client is not initialized.");
                    }
                    
                    var receiveMessageResponse = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest, cancellationToken);

                    if (receiveMessageResponse?.Messages == null)
                    {
                        // _logger.LogWarning("{Name}: {ConsumerName}: No messages received from Queue: {ConsumerSettingsSubscribeToSource}", nameof(AwsQueueConsumer), consumerName, messagingSettings.SubscribeToSource);
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
                        // _logger.LogDebug("**** {Name}: {ConsumerName}: {HandlerName} {Succeeded} ****", nameof(AwsQueueConsumer), consumerName, handlerName, success ? "SUCCEEDED" : "FAILED");
                        
                        if (!success) continue; // Undeleted messages will be retried by AWS SQS, or sent to the error queue if configured

                        if (!await _awsQueueService.DeleteMessageAsync(queueUrl, message.ReceiptHandle))
                        {
                            _logger.LogError("{Name}: {ConsumerName}: could not delete message on Queue: {ConsumerSettingsSubscribeToSource}", nameof(AwsQueueConsumer), consumerName, messagingSettings.SubscribeToSource);
                        }
                    }
                }

                _logger.LogInformation("**** CONSUMER STOPPED: {ConsumerName}", consumerName);
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("**** {Name}: {ConsumerName}: Stopped Queue: {ConsumerSettingsSubscribeToSource}", nameof(AwsQueueConsumer), consumerName, messagingSettings.SubscribeToSource);
            }
            catch (AggregateException ex)
            {
                foreach (var inner in ex.Flatten().InnerExceptions)
                {
                    _logger.LogError(inner, "**** {Name}: {ConsumerName}: AggregateException:", nameof(AwsQueueConsumer), consumerName);
                }
                _sqsClient?.Dispose();
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "**** {Name}: {ConsumerName}: Exception:", nameof(AwsQueueConsumer), consumerName);
                _sqsClient?.Dispose();
                throw;
            }
        }
    }
}
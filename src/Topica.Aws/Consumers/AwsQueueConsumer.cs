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
using Topica.Contracts;
using Topica.Settings;
using Message = Topica.Messages.Message;

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

        public async Task ConsumeAsync(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken)
        {
            await _topicProviderFactory.Create(MessagingPlatform.Aws).CreateTopicAsync(consumerSettings);

            Parallel.ForEach(Enumerable.Range(1, consumerSettings.NumberOfInstances), index =>
            {
                _retryPipeline.ExecuteAsync(x => StartAsync($"{consumerName}-({index})", consumerSettings, x), cancellationToken);
            });
        }

        private async ValueTask StartAsync(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken)
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

                var receiveMessageRequest = new ReceiveMessageRequest { QueueUrl = queueUrl, MaxNumberOfMessages = consumerSettings.AwsMaximumNumberOfMessages ?? 10};

                while (!cancellationToken.IsCancellationRequested)
                {
                    var receiveMessageResponse = await _client.ReceiveMessageAsync(receiveMessageRequest, cancellationToken);

                    if (receiveMessageResponse?.Messages == null)
                    {
                        await Task.Delay(1000, cancellationToken);
                        continue;
                    }
                    
                    foreach (var message in receiveMessageResponse.Messages)
                    {
                        _logger.LogDebug("SQS: Original Message from AWS: {SerializeObject}", JsonConvert.SerializeObject(message));
                        
                        //Serialise normal SQS message body
                        var baseMessage = JsonConvert.DeserializeObject<Message>(message.Body);

                        if (baseMessage == null)
                        {
                            _logger.LogWarning("SQS: message body could not be serialized into Message ({MessageId}): {MessageBody}", message.MessageId, message.Body);
                            continue;
                        }

                        var messageBody = message.Body;
                        
                        if (baseMessage.Type == "Notification")
                        {
                            //Otherwise serialise to an SnsMessage
                            var snsMessage = Amazon.SimpleNotificationService.Util.Message.ParseMessage(message.Body);
                            messageBody = snsMessage.MessageText;
                        }

                        var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync(consumerSettings.MessageToHandle, messageBody);
                        _logger.LogDebug("**** {AwsQueueConsumerName}: {ConsumerName}: {HandlerName} {Succeeded} ****", nameof(AwsQueueConsumer), consumerName, handlerName, success ? "SUCCEEDED" : "FAILED");
                        
                        if (!success) continue;

                        if (!await _awsQueueService.DeleteMessageAsync(queueUrl, message.ReceiptHandle))
                        {
                            _logger.LogError("{AwsQueueConsumerName}: {ConsumerName}: could not delete message on Queue: {ConsumerSettingsSubscribeToSource}", nameof(AwsQueueConsumer), consumerName, consumerSettings.SubscribeToSource);
                        }
                        else
                        {
                            _logger.LogDebug("{AwsQueueConsumerName}: {ConsumerName}: Success, deleting message on Queue: {ConsumerSettingsSubscribeToSource}", nameof(AwsQueueConsumer), consumerName, consumerSettings.SubscribeToSource);
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
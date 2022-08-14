using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Topica.Contracts;
using Topica.Settings;
using Message = Topica.Messages.Message;

namespace Topica.Aws.Queues
{
    public class AwsQueueConsumer : IConsumer
    {
        private readonly IAmazonSQS _client;
        private readonly IMessageHandlerExecutor _messageHandlerExecutor;
        private readonly IQueueProvider _queueProvider;
        private readonly ILogger<AwsQueueConsumer> _logger;

        public AwsQueueConsumer(IAmazonSQS client, IMessageHandlerExecutor messageHandlerExecutor, IQueueProvider queueProvider, ILogger<AwsQueueConsumer> logger)
        {
            _client = client;
            _messageHandlerExecutor = messageHandlerExecutor;
            _queueProvider = queueProvider;
            _logger = logger;
        }

        public Task ConsumeAsync(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken)
        {
            Parallel.ForEach(Enumerable.Range(1, consumerSettings.NumberOfInstances), index =>
            {
                StartAsync($"{consumerName}-({index})", consumerSettings, cancellationToken);
            });
            
            return Task.CompletedTask;
        }

        private async Task StartAsync(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken)
        {
            try
            {
                var queueUrl = await _queueProvider.GetQueueUrlAsync(consumerSettings.SubscribeToSource);

                if (string.IsNullOrWhiteSpace(queueUrl))
                {
                    var message = $"{nameof(AwsQueueConsumer)}: queue: {consumerSettings.SubscribeToSource} does not exist.";
                    _logger.LogError(message);

                    throw new ApplicationException(message);
                }

                _logger.LogInformation($"{nameof(AwsQueueConsumer)}:: {consumerName} started on Queue: {queueUrl}");

                var receiveMessageRequest = new ReceiveMessageRequest { QueueUrl = queueUrl, MaxNumberOfMessages = consumerSettings.AwsMaximumNumberOfMessages ?? 10};

                while (!cancellationToken.IsCancellationRequested)
                {
                    var receiveMessageResponse = await _client.ReceiveMessageAsync(receiveMessageRequest, cancellationToken);

                    foreach (var message in receiveMessageResponse.Messages)
                    {
                        _logger.LogDebug($"SQS: Original Message from AWS: {JsonConvert.SerializeObject(message)}");
                        
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
                        _logger.LogDebug($"**** {nameof(AwsQueueConsumer)}: {consumerName}: {handlerName} {(success ? "SUCCEEDED" : "FAILED")} ****");
                        
                        if (!success) continue;

                        if (!await _queueProvider.DeleteMessageAsync(queueUrl, message.ReceiptHandle))
                        {
                            _logger.LogError($"{nameof(AwsQueueConsumer)}: {consumerName}: could not delete message on Queue: {consumerSettings.SubscribeToSource}");
                        }
                        else
                        {
                            _logger.LogDebug($"{nameof(AwsQueueConsumer)}: {consumerName}: Success, deleting message on Queue: {consumerSettings.SubscribeToSource}");
                        }
                    }

                    await Task.Delay(250, cancellationToken);
                }

                _logger.LogInformation($"{nameof(AwsQueueConsumer)}: {consumerName}: Stopped Queue: {consumerSettings.SubscribeToSource}");
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation($"{nameof(AwsQueueConsumer)}: {consumerName}: Stopped Queue: {consumerSettings.SubscribeToSource}");
                _client.Dispose();
            }
            catch (AggregateException ex)
            {
                foreach (var inner in ex.Flatten().InnerExceptions)
                {
                    _logger.LogError(inner, $"{nameof(AwsQueueConsumer)}: {consumerName}: AggregateException:");
                }

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(AwsQueueConsumer)}: {consumerName}: Exception:");
                throw;
            }
        }
    }
}
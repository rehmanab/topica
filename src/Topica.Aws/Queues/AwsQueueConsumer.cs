using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Topica.Contracts;
using Topica.Messages;
using Topica.Settings;

namespace Topica.Aws.Queues
{
    public class AwsQueueConsumer : IConsumer
    {
        private readonly IMessageHandlerExecutor _messageHandlerExecutor;
        private readonly IQueueProvider _queueProvider;
        private readonly ILogger<AwsQueueConsumer> _logger;

        public AwsQueueConsumer(IMessageHandlerExecutor messageHandlerExecutor, IQueueProvider queueProvider, ILogger<AwsQueueConsumer> logger)
        {
            _messageHandlerExecutor = messageHandlerExecutor;
            _queueProvider = queueProvider;
            _logger = logger;
        }

        public async Task StartAsync<T>(string consumerName, ConsumerItemSettings consumerItemSettings, CancellationToken cancellationToken = default) where T : Message
        {
            try
            {
                var queueUrl = _queueProvider.GetQueueUrlAsync(consumerItemSettings.Source).Result;

                if (string.IsNullOrWhiteSpace(queueUrl))
                {
                    var message = $"SQS: QueueConsumer queue: {consumerItemSettings.Source} does not exist.";
                    _logger.LogError(message);

                    throw new ApplicationException(message);
                }

                _logger.LogInformation($"SQS: QueueConsumer Started: {consumerItemSettings.Source}");

                _logger.LogInformation($"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName} started on Queue: {consumerItemSettings.Source}");
                await foreach (var message in _queueProvider.StartReceive<T>(queueUrl, cancellationToken))
                {
                    if (message == null)
                    {
                        throw new Exception($"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName} - Received null message on Queue: {consumerItemSettings.Source}");
                    }

                    var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync(typeof(T).Name, JsonConvert.SerializeObject(message));
                    _logger.LogInformation($"**** {nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName}: {handlerName} {(success ? "SUCCEEDED" : "FAILED")} ****");

                    if (!success) continue;

                    if (!await _queueProvider.DeleteMessageAsync(queueUrl, message.ReceiptReference))
                    {
                        _logger.LogError($"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName}: could not delete message on Queue: {consumerItemSettings.Source}");
                    }
                    else
                    {
                        _logger.LogDebug($"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName}: Success, deleting message on Queue: {consumerItemSettings.Source}");
                    }
                }

                _logger.LogInformation($"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName}: Stopped on Queue: {consumerItemSettings.Source}");
            }
            catch (AggregateException ex)
            {
                foreach (var inner in ex.Flatten().InnerExceptions)
                {
                    _logger.LogError(inner, $"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName}: AggregateException:");
                }

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName}: Exception:");
                throw;
            }
        }
    }
}
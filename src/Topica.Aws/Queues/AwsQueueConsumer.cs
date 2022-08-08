using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Topica.Contracts;
using Topica.Messages;

namespace Topica.Aws.Queues
{
    public class AwsQueueConsumer : IQueueConsumer
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

        public async Task StartAsync<T>(string consumerName, string source, CancellationToken cancellationToken = default) where T : Message
        {
            try
            {
                var queueUrl = _queueProvider.GetQueueUrlAsync(source).Result;

                if (string.IsNullOrWhiteSpace(queueUrl))
                {
                    var message = $"SQS: QueueConsumer queue: {source} does not exist.";
                    _logger.LogError(message);

                    throw new ApplicationException(message);
                }

                _logger.LogInformation($"SQS: QueueConsumer Started: {source}");

                _logger.LogInformation($"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName} started on Queue: {source}");
                await foreach (var message in _queueProvider.StartReceive<T>(queueUrl, cancellationToken))
                {
                    if (message == null)
                    {
                        throw new Exception($"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName} - Received null message on Queue: {source}");
                    }

                    var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync(typeof(T).Name, JsonConvert.SerializeObject(message));
                    _logger.LogInformation($"**** {nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName}: {handlerName} {(success ? "SUCCEEDED" : "FAILED")} ****");

                    if (!success) continue;

                    if (!await _queueProvider.DeleteMessageAsync(queueUrl, message.ReceiptReference))
                    {
                        _logger.LogError($"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName}: could not delete message on Queue: {source}");
                    }
                    else
                    {
                        _logger.LogDebug($"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName}: Success, deleting message on Queue: {source}");
                    }
                }

                _logger.LogInformation($"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName}: Stopped on Queue: {source}");
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
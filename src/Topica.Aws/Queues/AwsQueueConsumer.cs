using System;
using System.Linq;
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

        public Task ConsumeAsync<T>(string consumerName, ConsumerItemSettings consumerItemSettings, int numberOfInstances, CancellationToken cancellationToken = default) where T : Message
        {
            Parallel.ForEach(Enumerable.Range(1, numberOfInstances), index =>
            {
                ConsumeAsync<T>($"{consumerName}-({index})", consumerItemSettings, cancellationToken);
            });
            
            return Task.CompletedTask;
        }

        public async Task ConsumeAsync<T>(string consumerName, ConsumerItemSettings consumerItemSettings, CancellationToken cancellationToken = default) where T : Message
        {
            try
            {
                var queueUrl = await _queueProvider.GetQueueUrlAsync(consumerItemSettings.Source);

                if (string.IsNullOrWhiteSpace(queueUrl))
                {
                    var message = $"{nameof(AwsQueueConsumer)}: QueueConsumer queue: {consumerItemSettings.Source} does not exist.";
                    _logger.LogError(message);

                    throw new ApplicationException(message);
                }

                _logger.LogInformation($"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName} started on Queue: {queueUrl}");
                await foreach (var message in _queueProvider.StartReceive<T>(queueUrl, cancellationToken))
                {
                    if (message == null)
                    {
                        throw new Exception($"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName} - Received null message on Queue: {queueUrl}");
                    }

                    var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync(typeof(T).Name, JsonConvert.SerializeObject(message));
                    _logger.LogInformation($"**** {nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName}: {handlerName} {(success ? "SUCCEEDED" : "FAILED")} ****");

                    if (!success) continue;

                    if (!await _queueProvider.DeleteMessageAsync(queueUrl, message.ReceiptReference))
                    {
                        _logger.LogError($"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName}: could not delete message on Queue: {queueUrl}");
                    }
                    else
                    {
                        _logger.LogDebug($"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName}: Success, deleting message on Queue: {queueUrl}");
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
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Aws.Messages;
using Topica.Contracts;

namespace Topica.Aws.Queues
{
    public class AwsQueueConsumer : IAwsQueueConsumer
    {
        private readonly IQueueProvider _queueProvider;
        private readonly ILogger<AwsQueueConsumer> _logger;

        private const int DefaultNumberOfThreads = 1;

        public AwsQueueConsumer(IQueueProvider queueProvider, ILogger<AwsQueueConsumer> logger)
        {
            _queueProvider = queueProvider;
            _logger = logger;
        }

        public async Task StartAsync<T>(string consumerName, string queueName, Func<IHandler<T>> handlerFactory, CancellationToken cancellationToken = default) where T : BaseSqsMessage
        {
            try
            {
                var queueUrl = _queueProvider.GetQueueUrlAsync(queueName).Result;

                if (string.IsNullOrWhiteSpace(queueUrl))
                {
                    var message = $"SQS: QueueConsumer queue: {queueName} does not exist.";
                    _logger.LogError(message);

                    throw new ApplicationException(message);
                }

                _logger.LogInformation($"SQS: QueueConsumer Started: {queueName}");

                var rnd = new Random(Guid.NewGuid().GetHashCode());

                var handler = handlerFactory();
                _logger.LogInformation($"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName} started on Queue: {queueName}");
                await foreach (var message in _queueProvider.StartReceive<T>(queueUrl, cancellationToken))
                {
                    if (message == null)
                    {
                        throw new Exception($"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName} - Received null message on Queue: {queueName}");
                    }

                    var success = await handler.Handle(message);

                    if (!success) continue;

                    if (!await _queueProvider.DeleteMessageAsync(queueUrl, message.ReceiptHandle))
                    {
                        _logger.LogError($"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName}: could not delete message on Queue: {queueName}");
                    }
                    else
                    {
                        _logger.LogDebug($"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName}: Success, deleting message on Queue: {queueName}");
                    }

                    // await Task.Delay(rnd.Next(500, 3000), cancellationToken);
                }

                _logger.LogInformation($"{nameof(AwsQueueConsumer)}: QueueConsumer: {consumerName}: Stopped on Queue: {queueName}");
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
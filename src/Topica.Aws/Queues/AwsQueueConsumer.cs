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

        public void Start<T>(string queueName, Func<IHandler<T>> handlerFactory, CancellationToken cancellationToken = default) where T : BaseSqsMessage
        {
            Start(queueName, DefaultNumberOfThreads, handlerFactory, cancellationToken);
        }

        public void Start<T>(string queueName, int numberOfThreads, Func<IHandler<T>> handlerFactory, CancellationToken cancellationToken = default) where T : BaseSqsMessage
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

                for (var i = 0; i < numberOfThreads; i++)
                {
                    var threadNumber = i + 1;
                    Task.Run(async () =>
                        {
                            var handler = handlerFactory();
                            _logger.LogInformation($"SQS: QueueConsumer thread: {threadNumber} started on Queue: {queueName}");
                            await foreach (var message in _queueProvider.StartReceive<T>(queueUrl, cancellationToken))
                            {
                                if (message == null)
                                {
                                    throw new Exception($"Received null message thread: {threadNumber} on Queue: {queueName}");
                                }

                                var success = await handler.Handle(message);

                                if (!success) continue;

                                if (!await _queueProvider.DeleteMessageAsync(queueUrl, message.ReceiptHandle))
                                {
                                    _logger.LogError($"SQS: could not delete message thread: {threadNumber} on Queue: {queueName}");
                                }
                                else
                                {
                                    _logger.LogDebug($"SQS: Success, deleting message thread: {threadNumber} on Queue: {queueName}");
                                }

                                await Task.Delay(rnd.Next(500, 3000), cancellationToken);
                            }

                            _logger.LogInformation($"SQS: QueueConsumer Stopped thread: {threadNumber} on Queue: {queueName}");
                        }, cancellationToken)
                        .ContinueWith(x =>
                        {
                            if (x.IsFaulted || x.Exception != null)
                            {
                                _logger.LogError(x.Exception, $"EXCEPTION: thread: {threadNumber} on Queue: {queueName}");
                            }
                        }, cancellationToken);
                }
            }
            catch (AggregateException ex)
            {
                foreach (var inner in ex.Flatten().InnerExceptions)
                {
                    _logger.LogError(inner, "SQS: AggregateException:");
                }

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQS: Exception:");
                throw;
            }
        }
    }
}
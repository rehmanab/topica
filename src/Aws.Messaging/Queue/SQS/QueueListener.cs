using System;
using System.Threading;
using System.Threading.Tasks;
using Aws.Messaging.Contracts;
using Aws.Messaging.Messages;
using Microsoft.Extensions.Logging;

namespace Aws.Messaging.Queue.SQS
{
    public class QueueListener : IQueueListener
    {
        private readonly IQueueProvider _queueProvider;
        private readonly ILogger<QueueListener> _logger;

        private const int DefaultNumberOfInstances = 1;

        public QueueListener(IQueueProvider queueProvider, ILogger<QueueListener> logger)
        {
            _queueProvider = queueProvider;
            _logger = logger;
        }

        public void Start<T>(string queueName, Func<IHandler<T>> handlerFactory, CancellationToken cancellationToken = default(CancellationToken)) where T : BaseSqsMessage
        {
            Start(queueName, DefaultNumberOfInstances, handlerFactory, cancellationToken);
        }

        public void Start<T>(string queueName, int numberOfInstances, Func<IHandler<T>> handlerFactory, CancellationToken cancellationToken = default(CancellationToken)) where T : BaseSqsMessage 
        {
            try
            {
                var queueUrl = _queueProvider.GetQueueUrlAsync(queueName).Result;

                if (string.IsNullOrWhiteSpace(queueUrl))
                {
                    var message = $"SQS: QueueListener queue: {queueName} does not exist.";
                    _logger.LogError(message);
                    
                    throw new ApplicationException(message);
                }

                _logger.LogInformation($"SQS: QueueListener Started: {queueName}");

                for (var i = 0; i < numberOfInstances; i++)
                {
                    Task.Run(async () =>
                    {
                        var handler = handlerFactory();
                        await foreach (var message in _queueProvider.StartReceive<T>(queueUrl, cancellationToken))
                        {
                            var success = await handler.Handle(message);

                            if (!success) continue;

                            _logger.LogDebug("SQS: Success, deleting message");
                            if (!await _queueProvider.DeleteMessageAsync(queueUrl, message.ReceiptHandle))
                            {
                                _logger.LogError("SQS: could not delete message");
                            }
                        }

                        _logger.LogInformation($"SQS: QueueListener Stopped: {queueName}");
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
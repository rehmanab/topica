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

        private const int DefaultNumberOfInstances = 1;

        public AwsQueueConsumer(IQueueProvider queueProvider, ILogger<AwsQueueConsumer> logger)
        {
            _queueProvider = queueProvider;
            _logger = logger;
        }

        public void Start<T>(string queueName, Func<IHandler<T>> handlerFactory, CancellationToken cancellationToken = default) where T : BaseSqsMessage
        {
            Start(queueName, DefaultNumberOfInstances, handlerFactory, cancellationToken);
        }

        public void Start<T>(string queueName, int numberOfInstances, Func<IHandler<T>> handlerFactory, CancellationToken cancellationToken = default) where T : BaseSqsMessage 
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

                for (var i = 0; i < numberOfInstances; i++)
                {
                    Task.Run(async () =>
                    {
                        var handler = handlerFactory();
                        await foreach (var message in _queueProvider.StartReceive<T>(queueUrl, cancellationToken))
                        {
                            if (message == null)
                            {
                                throw new Exception($"Received null message on: {queueName}");
                            }
                            
                            var success = await handler.Handle(message);

                            if (!success) continue;

                            _logger.LogDebug("SQS: Success, deleting message");
                            if (!await _queueProvider.DeleteMessageAsync(queueUrl, message.ReceiptHandle))
                            {
                                _logger.LogError("SQS: could not delete message");
                            }
                        }

                        _logger.LogInformation($"SQS: QueueConsumer Stopped: {queueName}");
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
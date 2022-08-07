using System;
using System.Threading;
using Topica.Aws.Messages;
using Topica.Contracts;

namespace Topica.Aws.Queues
{
    public interface IAwsQueueConsumer
    {
        void Start<T>(string queueName, Func<IHandler<T>> handlerFactory, CancellationToken cancellationToken = default) where T : BaseSqsMessage;
        void Start<T>(string queueName, int numberOfThreads, Func<IHandler<T>> handlerFactory, CancellationToken cancellationToken = default) where T : BaseSqsMessage;
    }
}
using System;
using System.Threading;
using Topica.Messages;

namespace Topica.Contracts
{
    public interface IQueueListener
    {
        void Start<T>(string queueName, Func<IHandler<T>> handlerFactory, CancellationToken cancellationToken = default(CancellationToken)) where T : BaseSqsMessage;
        void Start<T>(string queueName, int numberOfInstances, Func<IHandler<T>> handlerFactory, CancellationToken cancellationToken = default(CancellationToken)) where T : BaseSqsMessage;
    }
}
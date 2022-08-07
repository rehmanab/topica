using System;
using System.Threading;
using System.Threading.Tasks;
using Topica.Aws.Messages;
using Topica.Contracts;

namespace Topica.Aws.Queues
{
    public interface IAwsQueueConsumer
    {
        Task StartAsync<T>(string consumerName, string queueName, Func<IHandler<T>> handlerFactory, CancellationToken cancellationToken = default) where T : BaseSqsMessage;
    }
}
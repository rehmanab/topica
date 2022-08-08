using System.Threading;
using System.Threading.Tasks;
using Topica.Aws.Messages;

namespace Topica.Aws.Queues
{
    public interface IAwsQueueConsumer
    {
        Task StartAsync<T>(string consumerName, string queueName, CancellationToken cancellationToken = default) where T : BaseAwsMessage;
    }
}
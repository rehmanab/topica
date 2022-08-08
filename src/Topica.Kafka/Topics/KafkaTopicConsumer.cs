using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;
using Topica.Messages;

namespace Topica.Kafka.Topics
{
    public class KafkaTopicConsumer : IQueueConsumer
    {
        public async Task StartAsync<T>(string consumerName, string source, CancellationToken cancellationToken = default) where T : Message
        {
            
        }
    }
}
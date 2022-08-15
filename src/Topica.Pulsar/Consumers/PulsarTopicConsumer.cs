using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Pulsar.Consumers
{
    public class PulsarTopicConsumer : IConsumer
    {
        public PulsarTopicConsumer()
        {
            
        }
        
        public async Task ConsumeAsync(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken)
        {
            
        }
    }
}
using System.Threading.Tasks;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Pulsar.Creators
{
    public class PulsarTopicCreator : ITopicCreator
    {
        public PulsarTopicCreator()
        {
            
        }
        
        public MessagingPlatform MessagingPlatform => MessagingPlatform.Pulsar;
        
        public async Task<IConsumer> CreateTopic(ConsumerSettings settings)
        {
            throw new System.NotImplementedException();
        }
    }
}
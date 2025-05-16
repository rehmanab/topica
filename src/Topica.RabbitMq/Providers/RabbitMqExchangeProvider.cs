using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.RabbitMq.Contracts;
using Topica.RabbitMq.Models;
using Topica.RabbitMq.Requests;
using Topica.Settings;

namespace Topica.RabbitMq.Providers
{
    public class RabbitMqExchangeProvider(IRabbitMqManagementApiClient managementApiClient, ILogger<RabbitMqExchangeProvider> logger) : ITopicProvider
    {
        public MessagingPlatform MessagingPlatform => MessagingPlatform.RabbitMq;

        public async Task CreateTopicAsync(ConsumerSettings settings)
        {
            await CreateTopicAsync(settings.Source, settings.WithSubscribedQueues);
        }

        public async Task CreateTopicAsync(ProducerSettings settings)
        {
            await CreateTopicAsync(settings.Source, settings.WithSubscribedQueues);
        }

        private  async Task<object> CreateTopicAsync(string source, string[] withSubscribedQueues)
        {
            var queues = withSubscribedQueues.Select(subscribedQueue => new CreateRabbitMqQueueRequest
            {
                Name = subscribedQueue, Durable = true
            }).ToList();

            await managementApiClient.CreateAsync(source, true, ExchangeTypes.Fanout, queues);
            
            logger.LogInformation($"{nameof(RabbitMqExchangeProvider)}.{nameof(CreateTopicAsync)}: Created exchange {source}");
            
            return new object();
        }
    }
}
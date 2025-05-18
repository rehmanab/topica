using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.RabbitMq.Producers
{
    public class RabbitMqProducerBuilder(ITopicProviderFactory topicProviderFactory, ConnectionFactory rabbitMqConnectionFactory) : IProducerBuilder, IDisposable
    {
        private IChannel? _channel;

        public async Task<T> BuildProducerAsync<T>(string producerName, ProducerSettings producerSettings, CancellationToken cancellationToken)
        {
            await topicProviderFactory.Create(MessagingPlatform.RabbitMq).CreateTopicAsync(producerSettings);

            var connection = await rabbitMqConnectionFactory.CreateConnectionAsync(cancellationToken);
            _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
            
            return await Task.FromResult((T)_channel);
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}
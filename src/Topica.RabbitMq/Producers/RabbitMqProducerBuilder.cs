using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.RabbitMq.Producers
{
    public class RabbitMqProducerBuilder : IProducerBuilder, IDisposable
    {
        private IModel? _channel;
        private readonly ConnectionFactory _rabbitMqConnectionFactory;

        public RabbitMqProducerBuilder(ConnectionFactory rabbitMqConnectionFactory)
        {
            _rabbitMqConnectionFactory = rabbitMqConnectionFactory;
        }
        
        public async Task<T> BuildProducerAsync<T>(string producerName, ProducerSettings producerSettings, CancellationToken cancellationToken)
        {
            var connection = _rabbitMqConnectionFactory.CreateConnection();
            _channel = connection.CreateModel();
            
            return await Task.FromResult((T)_channel);
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}
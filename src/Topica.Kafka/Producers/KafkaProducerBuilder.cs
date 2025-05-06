using System;
using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Kafka.Producers
{
    public class KafkaProducerBuilder : IProducerBuilder, IDisposable
    {
        public Task<T> BuildProducerAsync<T>(string producerName, ProducerSettings producerSettings, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
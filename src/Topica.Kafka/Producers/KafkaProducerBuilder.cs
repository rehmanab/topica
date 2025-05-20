using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Kafka.Producers
{
    public class KafkaProducerBuilder : IProducerBuilder, IDisposable
    {
        public async Task<T> BuildProducerAsync<T>(string producerName, ProducerSettings producerSettings, CancellationToken cancellationToken)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = string.Join(",", producerSettings.KafkaBootstrapServers),
                //BootstrapServers = "dockerhost:30005",
                //SaslMechanism = SaslMechanism.Plain
                //SecurityProtocol = SecurityProtocol.Ssl
            };

            var producer = new ProducerBuilder<string, string>(config)
                //.SetValueSerializer(Serializers.ByteArray)
                .Build();

            await Task.CompletedTask;
            
            return (T)producer;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
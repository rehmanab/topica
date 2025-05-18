// using System;
// using System.Threading;
// using System.Threading.Tasks;
// using Topica.Contracts;
// using Topica.Settings;
//
// namespace Topica.Aws.Producers
// {
//     public class AwsProducerBuilder(ConnectionFactory rabbitMqConnectionFactory) : IProducerBuilder, IDisposable
//     {
//         private IChannel? _channel;
//
//         public async Task<T> BuildProducerAsync<T>(string producerName, ProducerSettings producerSettings, CancellationToken cancellationToken)
//         {
//             var connection = await rabbitMqConnectionFactory.CreateConnectionAsync(cancellationToken);
//             _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
//             
//             return await Task.FromResult((T)_channel);
//         }
//
//         public void Dispose()
//         {
//             _channel?.Dispose();
//         }
//     }
// }
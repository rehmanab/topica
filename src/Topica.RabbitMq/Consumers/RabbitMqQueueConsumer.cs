using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.RabbitMq.Consumers
{
    public class RabbitMqQueueConsumer : IConsumer, IDisposable
    {
        private readonly ConnectionFactory _rabbitMqConnectionFactory;
        private readonly IMessageHandlerExecutor _messageHandlerExecutor;
        private readonly ILogger<RabbitMqQueueConsumer> _logger;
        private IModel _channel;

        public RabbitMqQueueConsumer(ConnectionFactory rabbitMqConnectionFactory, IMessageHandlerExecutor messageHandlerExecutor, ILogger<RabbitMqQueueConsumer> logger)
        {
            _rabbitMqConnectionFactory = rabbitMqConnectionFactory;
            _messageHandlerExecutor = messageHandlerExecutor;
            _logger = logger;
        }
        
        public Task ConsumeAsync(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken)
        {
            Parallel.ForEach(Enumerable.Range(1, consumerSettings.NumberOfInstances), index =>
            {
                StartAsync($"{consumerName}-({index})", consumerSettings, cancellationToken);
            });
            
            return Task.CompletedTask;
        }

        private async Task StartAsync(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken)
        {
            var connection = _rabbitMqConnectionFactory.CreateConnection();
            _channel = connection.CreateModel();
            
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (sender, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync(consumerSettings.MessageToHandle, message);
                _logger.LogInformation($"**** {nameof(RabbitMqQueueConsumer)}: {consumerName}: {handlerName}: Queue: {consumerSettings.SubscribeToSource}: {(success ? "SUCCEEDED" : "FAILED")} ****");
            };

            await Task.Run(() =>
            {
                _logger.LogInformation($"{nameof(RabbitMqQueueConsumer)}: {consumerName} started on Queue: {consumerSettings.SubscribeToSource}");

                _channel.BasicConsume(consumerSettings.SubscribeToSource, true, consumer);
            }, cancellationToken)
                .ContinueWith(x =>
                {
                    if (x.IsFaulted || x.Exception != null)
                    {
                        _logger.LogError(x.Exception, "{ClassName}: {ConsumerName}: Error", nameof(RabbitMqQueueConsumer), consumerName);      
                    }
                }, cancellationToken);
        }

        public void Dispose()
        {
            _channel.Dispose();
            _logger.LogInformation($"{nameof(RabbitMqQueueConsumer)}: Disposed");

        }
    }
}
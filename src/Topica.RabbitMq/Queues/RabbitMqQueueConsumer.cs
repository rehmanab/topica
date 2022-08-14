using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Topica.Contracts;
using Topica.Messages;
using Topica.Settings;

namespace Topica.RabbitMq.Queues
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
        
        public Task ConsumeAsync<T>(string consumerName, ConsumerItemSettings consumerItemSettings, int numberOfInstances, CancellationToken cancellationToken = default) where T : Message
        {
            Parallel.ForEach(Enumerable.Range(1, numberOfInstances), index =>
            {
                ConsumeAsync<T>($"{Assembly.GetExecutingAssembly().GetName().Name}-{nameof(T)}-({index})", consumerItemSettings, cancellationToken);
            });
            
            return Task.CompletedTask;
        }

        public async Task ConsumeAsync<T>(string consumerName, ConsumerItemSettings consumerItemSettings, CancellationToken cancellationToken = default) where T : Message
        {
            var connection = _rabbitMqConnectionFactory.CreateConnection();
            _channel = connection.CreateModel();
            
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (sender, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync(typeof(T).Name, message);
                _logger.LogInformation($"**** {nameof(RabbitMqQueueConsumer)}: {consumerName}: {handlerName} {(success ? "SUCCEEDED" : "FAILED")} ****");
            };

            await Task.Run(() =>
            {
                _logger.LogInformation($"{nameof(RabbitMqQueueConsumer)}: {consumerName} started on Queue: {consumerItemSettings.Source}");

                _channel.BasicConsume(consumerItemSettings.Source, true, consumer);
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
        }
    }
}
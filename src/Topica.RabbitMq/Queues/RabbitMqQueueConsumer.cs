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
using Topica.RabbitMq.Settings;
using Topica.Settings;

namespace Topica.RabbitMq.Queues
{
    public class RabbitMqQueueConsumer : IConsumer, IDisposable
    {
        private readonly IMessageHandlerExecutor _messageHandlerExecutor;
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<RabbitMqQueueConsumer> _logger;
        private IModel _channel;

        public RabbitMqQueueConsumer(IMessageHandlerExecutor messageHandlerExecutor, RabbitMqSettings settings, ILogger<RabbitMqQueueConsumer> logger)
        {
            _messageHandlerExecutor = messageHandlerExecutor;
            _settings = settings;
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
            var factory = new ConnectionFactory
            {
                Uri = new Uri($"{_settings.Scheme}{Uri.SchemeDelimiter}{_settings.UserName}:{_settings.Password}@{_settings.Hostname}:{_settings.Port}/{_settings.VHost}"),
                DispatchConsumersAsync = true,
                RequestedHeartbeat = TimeSpan.FromSeconds(10),
                AutomaticRecoveryEnabled = true
            };
            
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (sender, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync(typeof(T).Name, message);
                _logger.LogInformation($"**** {nameof(RabbitMqQueueConsumer)}: QueueConsumer: {consumerName}: {handlerName} {(success ? "SUCCEEDED" : "FAILED")} ****");
            };

            await Task.Run(() =>
            {
                _logger.LogInformation($"{nameof(RabbitMqQueueConsumer)}: QueueConsumer: {consumerName} started on Queue: {consumerItemSettings.Source}");

                _channel.BasicConsume(consumerItemSettings.Source, true, consumer);
            }, cancellationToken)
                .ContinueWith(x =>
                {
                    if (x.IsFaulted || x.Exception != null)
                    {
                        _logger.LogError(x.Exception, "{ClassName}: TopicConsumer: {ConsumerName}: Error", nameof(RabbitMqQueueConsumer), consumerName);      
                    }
                }, cancellationToken);
        }

        public void Dispose()
        {
            _channel.Dispose();
        }
    }
}
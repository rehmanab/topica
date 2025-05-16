using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.RabbitMq.Consumers
{
    public class RabbitMqQueueConsumer : IConsumer, IDisposable
    {
        private readonly ITopicProviderFactory _topicProviderFactory;
        private readonly ConnectionFactory _rabbitMqConnectionFactory;
        private readonly IMessageHandlerExecutor _messageHandlerExecutor;
        private readonly ResiliencePipeline _retryPipeline;
        private readonly ILogger<RabbitMqQueueConsumer> _logger;
        private IModel? _channel = null;

        public RabbitMqQueueConsumer(ITopicProviderFactory topicProviderFactory, ConnectionFactory rabbitMqConnectionFactory, IMessageHandlerExecutor messageHandlerExecutor, ILogger<RabbitMqQueueConsumer> logger)
        {
            _topicProviderFactory = topicProviderFactory;
            _rabbitMqConnectionFactory = rabbitMqConnectionFactory;
            _messageHandlerExecutor = messageHandlerExecutor;
            _retryPipeline = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Constant,
                Delay = TimeSpan.FromSeconds(5),
                MaxRetryAttempts = int.MaxValue,
                OnRetry = args =>
                {
                    logger.LogWarning("Retrying: {ArgsAttemptNumber} in {RetryDelayTotalSeconds} seconds", args.AttemptNumber + 1, args.RetryDelay.TotalSeconds);
                    return default;
                }
            }).Build();
            _logger = logger;
        }

        public Task ConsumeAsync<T>(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken) where T : IHandler
        {
            throw new NotImplementedException();
        }

        public Task ConsumeAsync(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken)
        {
            Parallel.ForEach(Enumerable.Range(1, consumerSettings.NumberOfInstances), index =>
            {
                _retryPipeline.ExecuteAsync(x => StartAsync($"{consumerName}-({index})", consumerSettings, x), cancellationToken);
            });
            
            return Task.CompletedTask;
        }

        private async ValueTask StartAsync(string consumerName, ConsumerSettings consumerSettings, CancellationToken cancellationToken)
        {
            try
            {
                await _topicProviderFactory.Create(MessagingPlatform.RabbitMq).CreateTopicAsync(consumerSettings);

                var connection = _rabbitMqConnectionFactory.CreateConnection();
                _channel = connection.CreateModel();

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += async (sender, e) =>
                {
                    var body = e.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync(consumerSettings.MessageToHandle, message);
                    // _logger.LogInformation("**** {RabbitMqQueueConsumerName}: {ConsumerName}: {HandlerName}: Queue: {ConsumerSettingsSubscribeToSource}: {Succeeded} ****", nameof(RabbitMqQueueConsumer), consumerName, handlerName, consumerSettings.SubscribeToSource, success ? "SUCCEEDED" : "FAILED");
                };

                await Task.Run(() =>
                    {
                        _logger.LogInformation("{RabbitMqQueueConsumerName}: {ConsumerName} started on Queue: {ConsumerSettingsSubscribeToSource}", nameof(RabbitMqQueueConsumer), consumerName, consumerSettings.SubscribeToSource);

                        _channel.BasicConsume(consumerSettings.SubscribeToSource, true, consumer);
                    }, cancellationToken)
                    .ContinueWith(x =>
                    {
                        if ((x.IsFaulted || x.Exception != null) && !x.IsCanceled)
                        {
                            _logger.LogError(x.Exception, "{ClassName}: {ConsumerName}: Error", nameof(RabbitMqQueueConsumer), consumerName);
                        }
                    }, cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "{ClassName}: {ConsumerName}: Error", nameof(RabbitMqQueueConsumer), consumerName);
                throw;
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _logger.LogInformation("{RabbitMqQueueConsumerName}: Disposed", nameof(RabbitMqQueueConsumer));
        }
    }
}
using System;
using System.Collections.Generic;
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
    public class RabbitMqQueueConsumer : IConsumer
    {
        private readonly ConnectionFactory _rabbitMqConnectionFactory;
        private readonly IMessageHandlerExecutor _messageHandlerExecutor;
        private readonly MessagingSettings _messagingSettings;
        private readonly ResiliencePipeline _retryPipeline;
        private readonly ILogger _logger;
        private IChannel? _channel;

        public RabbitMqQueueConsumer(ConnectionFactory rabbitMqConnectionFactory, IMessageHandlerExecutor messageHandlerExecutor, MessagingSettings messagingSettings, ILogger logger)
        {
            _rabbitMqConnectionFactory = rabbitMqConnectionFactory;
            _messageHandlerExecutor = messageHandlerExecutor;
            _messagingSettings = messagingSettings;
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

        public async Task ConsumeAsync(CancellationToken cancellationToken)
        {
            Parallel.ForEach(Enumerable.Range(1, _messagingSettings.NumberOfInstances), index =>
            {
                _retryPipeline.ExecuteAsync(x => StartAsync($"{_messagingSettings.WorkerName}-({index})", x), cancellationToken);
            });
        }

        private async ValueTask StartAsync(string consumerName, CancellationToken cancellationToken)
        {
            try
            {
                var connection = await _rabbitMqConnectionFactory.CreateConnectionAsync(cancellationToken);
                _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (sender, e) =>
                {
                    var body = e.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    var (handlerName, success) = await _messageHandlerExecutor.ExecuteHandlerAsync(message, e.BasicProperties.Headers?.ToDictionary(x => x.Key, x =>
                    {
                        if(x.Value is byte[] bytes)
                        {
                            return Encoding.UTF8.GetString(bytes) ?? "";
                        }
                        return x.Value?.ToString() ?? "";
                    }) ?? new Dictionary<string, string>());
                    
                    // _logger.LogInformation("**** {RabbitMqQueueConsumerName}: {ConsumerName}: {HandlerName}: Queue: {ConsumerSettingsSubscribeToSource}: {Succeeded} ****", nameof(RabbitMqQueueConsumer), consumerName, handlerName, messagingSettings.SubscribeToSource, success ? "SUCCEEDED" : "FAILED");
                };

                await Task.Run(() =>
                    {
                        _logger.LogInformation("**** CONSUMER STARTED:{ConsumerName} started on Queue: {ConsumerSettingsSubscribeToSource}", consumerName, _messagingSettings.SubscribeToSource);

                        _channel.BasicConsumeAsync(_messagingSettings.SubscribeToSource, true, consumer, cancellationToken: cancellationToken);
                    }, cancellationToken)
                    .ContinueWith(x =>
                    {
                        if ((x.IsFaulted || x.Exception != null) && !x.IsCanceled)
                        {
                            _logger.LogError(x.Exception, "**** ERROR: {ClassName}: {ConsumerName}: Error", nameof(RabbitMqQueueConsumer), consumerName);
                        }
                    }, cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "**** ERROR: {ClassName}: {ConsumerName}: Error", nameof(RabbitMqQueueConsumer), consumerName);
                throw;
            }
        }
    }
}
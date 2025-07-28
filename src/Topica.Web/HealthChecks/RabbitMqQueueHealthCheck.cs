using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Topica.Messages;
using Topica.RabbitMq.Contracts;
using Topica.Web.Settings;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Topica.Web.HealthChecks;

public class RabbitMqQueueHealthCheck(IRabbitMqManagementApiClient managementApiClient, ConnectionFactory rabbitMqConnectionFactory, HealthCheckSettings healthCheckSettings) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!healthCheckSettings.HealthCheckEnabled.RabbitMqQueue) return HealthCheckResult.Degraded($"{nameof(RabbitMqQueueHealthCheck)} is Disabled");

        const string queueName = "topica_rmq_queue_health_check_web_queue_1";

        var sw = Stopwatch.StartNew();

        try
        {
            await managementApiClient.CreateVHostIfNotExistAsync();
            await managementApiClient.CreateQueueAsync(queueName, true);

            await using var connection = await rabbitMqConnectionFactory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            var testMessageName = Guid.NewGuid().ToString();

            await channel.BasicPublishAsync(string.Empty, queueName, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new BaseMessage
            {
                ConversationId = Guid.NewGuid(),
                EventId = 1,
                EventName = testMessageName,
                Type = nameof(BaseMessage),
                MessageGroupId = Guid.NewGuid().ToString()
            })), cancellationToken);

            var success = false;
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (_, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                success = JsonSerializer.Deserialize<BaseMessage>(message)?.EventName == testMessageName;
                return Task.CompletedTask;
            };
            
            while (!cancellationToken.IsCancellationRequested && !success)
            {
                await channel.BasicConsumeAsync(queueName, true, consumer, cancellationToken: cancellationToken);
            }

            return success
                ? HealthCheckResult.Healthy("Published, Subscribed to Queue: Success", data: new Dictionary<string, object>
                {
                    { "SendQueueName", queueName }
                })
                : HealthCheckResult.Unhealthy("Failed Queue health - did not receive message", data: new Dictionary<string, object>
                {
                    { "QueueName", queueName }
                });
            ;
        }
        catch (Exception ex) when (ex is TimeoutException or TaskCanceledException or OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy($"Timeout/Task Cancelled while checking Queue health. ({sw.Elapsed})", ex, data: new Dictionary<string, object>
            {
                { "QueueName", queueName },
                { "ExceptionName", ex.GetType().FullName ?? ex.GetType().Name },
                { "ExceptionMessage", ex.Message },
                { "ExceptionStackTrace", ex.StackTrace ?? string.Empty },
            });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("An error occurred while checking Queue health.", ex, data: new Dictionary<string, object>
            {
                { "QueueName", queueName },
                { "ExceptionName", ex.GetType().FullName ?? ex.GetType().Name },
                { "ExceptionMessage", ex.Message },
                { "ExceptionStackTrace", ex.StackTrace ?? string.Empty },
            });
        }
        finally
        {
            sw.Reset();
        }
    }
}
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Pulsar.Client.Api;
using Pulsar.Client.Common;
using Topica.Messages;
using Topica.Pulsar.Contracts;
using TimeoutException = System.TimeoutException;

namespace Topica.Web.HealthChecks;

public class PulsarHealthCheck(IPulsarService pulsarService, PulsarClientBuilder pulsarClientBuilder) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        const string topicName = "topica_pulsar_topic_health_check_web_topic_1";

        var sw = Stopwatch.StartNew();

        try
        {
            const string tenant = "public";
            const string @namespace = "topica";
            
            await pulsarService.CreateTenantAsync(tenant, cancellationToken);
            await pulsarService.CreateNamespaceAsync(tenant, @namespace, cancellationToken);
            await pulsarService.CreatePartitionedTopicAsync(tenant, @namespace, topicName, 6, cancellationToken: cancellationToken);

            var client = await pulsarClientBuilder.BuildAsync();
            await using var consumer = await client.NewConsumer()
                .Topic($"persistent://{tenant}/{@namespace}/{topicName}")
                .ConsumerName("topica_pulsar_health_check_web_consumer_1") // Consumer name is used to identify the consumer, if it is unique, it will act as a new subscriber
                .SubscriptionName("topica_pulsar_health_check_1") // will act as a new subscriber and read all messages if the name is unique
                .SubscriptionType(SubscriptionType.Shared) // If the topic is partitioned, then shared will allow other concurrent consumers (scale horizontally), with the same subscription name to split the messages between them
                .AcknowledgementsGroupTime(TimeSpan.FromSeconds(5)) // Acknowledgements will be sent immediately after processing the message, no batching, set this higher if you want to batch acknowledgements and less network traffic
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest) //Earliest will read unread, Latest will read live incoming messages only
                .SubscribeAsync();

            await using var producer = await client.NewProducer(Schema.BYTES())
                .ProducerName("topica_pulsar_health_check_web_producer_1")
                .Topic($"persistent://{tenant}/{@namespace}/{topicName}")
                .BlockIfQueueFull(true)
                .MaxPendingMessages(int.MaxValue)
                .MaxPendingMessagesAcrossPartitions(int.MaxValue)
                .EnableBatching(false)
                .EnableChunking(false) // Big messages are chuncked into smaller pieces
                .BatchingMaxMessages(10) // Batch, consumer will only ack messages after consumer has read all messages in the batch
                .BatchingMaxPublishDelay(TimeSpan.FromMilliseconds(500)) // Will delay upto this value before sending batch. Have to wait at least this amount before disposing
                .CreateAsync();

            var testMessageName = Guid.NewGuid().ToString();

            var message = new BaseMessage
            {
                ConversationId = Guid.NewGuid(),
                EventId = 1,
                EventName = testMessageName,
                Type = nameof(BaseMessage),
            };
            
            await producer.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
            
            // Takes a while for the producer, consumer to startup, takes message to be consumed, 15 - 30 seconds
            var success = false;
            while (!cancellationToken.IsCancellationRequested && !success)
            {
                var receivedMessage = await consumer.ReceiveAsync(cancellationToken);
                var baseMessage = JsonConvert.DeserializeObject<BaseMessage>(Encoding.UTF8.GetString(receivedMessage.Data));
                success = baseMessage != null && baseMessage.EventName == testMessageName;
                await consumer.AcknowledgeAsync(receivedMessage.MessageId);
            }
            
            return success
                ? HealthCheckResult.Healthy("Published, Subscribed to Topic: Success", data: new Dictionary<string, object>
                {
                    { "SendTopicName", topicName },
                })
                : HealthCheckResult.Unhealthy("Failed Topic health - did not receive message", data: new Dictionary<string, object>
                {
                    { "TopicName", topicName },
                });
        }
        catch (Exception ex) when (ex is TimeoutException or TaskCanceledException or OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy($"Timeout/Task Cancelled while checking Topic health. ({sw.Elapsed})", ex, data: new Dictionary<string, object>
            {
                { "TopicName", topicName },
                { "ExceptionName", ex.GetType().FullName ?? ex.GetType().Name },
                { "ExceptionMessage", ex.Message },
                { "ExceptionStackTrace", ex.StackTrace ?? string.Empty },
            });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"An error occurred while checking Topic health. ({sw.Elapsed})", ex, data: new Dictionary<string, object>
            {
                { "TopicName", topicName },
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
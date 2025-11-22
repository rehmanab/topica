using System.Diagnostics;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Topica.Messages;
using Topica.Web.Settings;

namespace Topica.Web.HealthChecks;

public class KafkaHealthCheck(KafkaHostSettings hostSettings, IWebHostEnvironment env) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        var topicName = $"topic_health_check_web_topic_{env.EnvironmentName.ToLower()}";

        var sw = Stopwatch.StartNew();

        try
        {
            using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = string.Join(",", hostSettings.BootstrapServers) }).Build();

            var meta = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

            if (!meta.Topics.Any(x => string.Equals(topicName, x.Topic, StringComparison.CurrentCultureIgnoreCase)))
            {
                await adminClient.CreateTopicsAsync([
                    new TopicSpecification { Name = topicName, ReplicationFactor = 1, NumPartitions = 6 }
                ]);
            }
            
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = string.Join(",", hostSettings.BootstrapServers),
                
                // Each unique consumer group will handle a share of the messages for that Topic
                // e.g. group1 with 10 consumers, share the messages
                // group2 will be like a new subscribed queue, and get all the messages
                // So each consumer GroupId is a subscribed queue
                // https://www.confluent.io/blog/configuring-apache-kafka-consumer-group-ids/
                GroupId = "topica_kafka_health_check_web_group_dev",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                SaslMechanism = SaslMechanism.Plain
                //SecurityProtocol = SecurityProtocol.Ssl
            };
            using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
            consumer.Subscribe(topicName);
            
            var producerConfig = new ProducerConfig
            {
                // TransactionalId = consumerName, // Doesn't work, maybe need to set topic as transactional (Kafka throws "Erroneous state") error
                BootstrapServers = string.Join(",", hostSettings.BootstrapServers),
                //SaslMechanism = SaslMechanism.Plain
                //SecurityProtocol = SecurityProtocol.Ssl
            };
            using var producer = new ProducerBuilder<string, string>(producerConfig).Build();

            var testMessageName = Guid.NewGuid().ToString();

            var message = new BaseMessage
            {
                ConversationId = Guid.NewGuid(),
                EventId = 1,
                EventName = testMessageName,
                Type = nameof(BaseMessage),
            };
            
            var result = await producer.ProduceAsync(topicName, new Message<string, string>
            {
                Key = message.GetType().Name,
                Value = JsonConvert.SerializeObject(message)
            }, cancellationToken);
            
            // Takes a while for the producer, consumer to startup, takes message to be consumed, 15 - 30 seconds
            var success = false;
            while (!cancellationToken.IsCancellationRequested && !success)
            {
                var receivedMessage = consumer.Consume(cancellationToken);
                var baseMessage = JsonConvert.DeserializeObject<BaseMessage>(receivedMessage.Message.Value);
                success = baseMessage != null && baseMessage.EventName == testMessageName;
                consumer.Commit(receivedMessage);
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
using System.Diagnostics;
using System.Text;
using Amazon.Auth.AccessControlPolicy;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Topica.Messages;
using Topica.RabbitMq.Contracts;
using Topica.RabbitMq.Models;
using Topica.RabbitMq.Requests;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Topica.Web.HealthChecks;

public class RabbitMqTopicHealthCheck(IRabbitMqManagementApiClient managementApiClient, ConnectionFactory rabbitMqConnectionFactory, IWebHostEnvironment env) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var topicName = $"topica_rmq_topic_health_check_web_topic_{env.EnvironmentName.ToLower()}";
        var subscribedQueueName = $"topica_rmq_topic_health_check_web_queue_{env.EnvironmentName.ToLower()}";

        var sw = Stopwatch.StartNew();

        try
        {
            var createRabbitMqQueueRequest = new CreateRabbitMqQueueRequest
            {
                Name = subscribedQueueName, Durable = true, RoutingKey = $"{subscribedQueueName}_routing_key"
            };

            await managementApiClient.CreateVHostIfNotExistAsync();
            await managementApiClient.CreateExchangeAndBindingsAsync(topicName, true, ExchangeTypes.Fanout, [createRabbitMqQueueRequest]);

            await using var connection = await rabbitMqConnectionFactory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            var testMessageName = Guid.NewGuid().ToString();

            await channel.BasicPublishAsync(topicName, "", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new BaseMessage
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
                await channel.BasicConsumeAsync(subscribedQueueName, true, consumer, cancellationToken: cancellationToken);
            }

            return success
                ? HealthCheckResult.Healthy("Published, Subscribed to Topic: Success", data: new Dictionary<string, object>
                {
                    { "SendTopicName", topicName },
                    { "ReceiveQueueName", subscribedQueueName },
                })
                : HealthCheckResult.Unhealthy("Failed Topic health - did not receive message", data: new Dictionary<string, object>
                {
                    { "TopicName", topicName },
                    { "ReceiveQueueName", subscribedQueueName },
                });
        }
        catch (Exception ex) when (ex is TimeoutException or TaskCanceledException or OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy($"Timeout/Task Cancelled while checking Topic health. ({sw.Elapsed})", ex, data: new Dictionary<string, object>
            {
                { "TopicName", topicName },
                { "ReceiveQueueName", subscribedQueueName },
                { "ExceptionName", ex.GetType().FullName ?? ex.GetType().Name },
                { "ExceptionMessage", ex.Message },
                { "ExceptionStackTrace", ex.StackTrace ?? string.Empty },
            });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("An error occurred while checking Topic health.", ex, data: new Dictionary<string, object>
            {
                { "TopicName", topicName },
                { "ReceiveQueueName", subscribedQueueName },
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

    private static string BuildQueueAllowPolicyForTopicToSendMessage(string queueArn, string topicArn)
    {
        const Statement.StatementEffect statementEffect = Statement.StatementEffect.Allow;
        var sourceArnWildcard = CreateResourceArnWildcard(topicArn);
        var principals = new[] { Principal.AllUsers };
        var actionIdentifiers = new[] { new ActionIdentifier("sqs:SendMessage") };

        return new Policy()
            .WithStatements(new Statement(statementEffect)
                .WithResources(new Resource(queueArn))
                .WithConditions(ConditionFactory.NewSourceArnCondition(sourceArnWildcard))
                .WithPrincipals(principals)
                .WithActionIdentifiers(actionIdentifiers)).ToJson();
    }

    private static string CreateResourceArnWildcard(string resourceArn)
    {
        if (string.IsNullOrWhiteSpace(resourceArn) ||
            !resourceArn.StartsWith("arn", StringComparison.InvariantCultureIgnoreCase))
        {
            throw new ApplicationException($"AwsPolicy: Seems not to be a valid ARN: {resourceArn}");
        }

        var index = resourceArn.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);

        if (index > 0)
            resourceArn = resourceArn.Substring(0, index + 1);
        else
            throw new ApplicationException($"AwsPolicy: Seems not to be a valid ARN: {resourceArn}");

        return resourceArn + "*";
    }
}
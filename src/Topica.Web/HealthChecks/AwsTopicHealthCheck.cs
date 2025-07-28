using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Amazon.Auth.AccessControlPolicy;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Topica.Aws.Messages;
using Topica.Aws.Queues;
using Topica.Messages;
using Topica.Web.Settings;

namespace Topica.Web.HealthChecks;

public class AwsTopicHealthCheck(IAmazonSimpleNotificationService snsClient, IAmazonSQS sqsClient, HealthCheckSettings healthCheckSettings) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!healthCheckSettings.HealthCheckEnabled.AwsTopic) return HealthCheckResult.Degraded($"{nameof(AwsTopicHealthCheck)} is Disabled");

        const string topicName = "topica_aws_topic_health_check_web_topic_1";
        const string subscribedQueueName = "topica_aws_topic_health_check_web_queue_1";

        var sw = Stopwatch.StartNew();

        try
        {
            var createTopicResponse = await snsClient.CreateTopicAsync(topicName, cancellationToken);
            if (createTopicResponse is null || createTopicResponse.HttpStatusCode != HttpStatusCode.OK || string.IsNullOrWhiteSpace(createTopicResponse.TopicArn))
            {
                return HealthCheckResult.Unhealthy("Failed to create or retrieve Topic ARN.", data: new Dictionary<string, object>
                {
                    { "TopicName", topicName }
                });
            }

            var createQueueResponse = await sqsClient.CreateQueueAsync(subscribedQueueName, cancellationToken);

            if (createQueueResponse is null || createQueueResponse.HttpStatusCode != HttpStatusCode.OK || string.IsNullOrWhiteSpace(createQueueResponse.QueueUrl))
            {
                return HealthCheckResult.Unhealthy("Failed to create or retrieve Queue URL.", data: new Dictionary<string, object>
                {
                    { "QueueName", subscribedQueueName }
                });
            }

            var queueAttributes = (await sqsClient.GetQueueAttributesAsync(createQueueResponse.QueueUrl, ["All"], cancellationToken)).Attributes;
            queueAttributes.Add("QueueUrl", createQueueResponse.QueueUrl);
            var queueArn = queueAttributes[AwsQueueAttributes.QueueArnName];
            await snsClient.SubscribeAsync(createTopicResponse.TopicArn, "sqs", queueArn, cancellationToken);

            var accessPolicy = BuildQueueAllowPolicyForTopicToSendMessage(queueArn, createTopicResponse.TopicArn);
            var awsSqsConfiguration = new AwsSqsConfiguration { QueueAttributes = new AwsQueueAttributes { Policy = accessPolicy } };

            var setQueueAttributesResponse = await sqsClient.SetQueueAttributesAsync(createQueueResponse.QueueUrl, awsSqsConfiguration.QueueAttributes.GetAttributeDictionary(), cancellationToken);

            if (setQueueAttributesResponse is null || setQueueAttributesResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                return HealthCheckResult.Unhealthy("Failed to change Queue attributes to subscribe to Topic.", data: new Dictionary<string, object>
                {
                    { "TopicName", topicName },
                    { "QueueName", subscribedQueueName },
                });
            }

            var testMessageName = Guid.NewGuid().ToString();

            var sendMessageResponse = await snsClient.PublishAsync(createTopicResponse.TopicArn, JsonSerializer.Serialize(new BaseMessage
            {
                ConversationId = Guid.NewGuid(),
                EventId = 1,
                EventName = testMessageName,
                Type = nameof(BaseMessage),
            }), cancellationToken);

            if (sendMessageResponse is null || sendMessageResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                return HealthCheckResult.Unhealthy("Failed to send message to Topic.", data: new Dictionary<string, object>
                {
                    { "TopicName", topicName }
                });
            }

            var receiveMessageResponse = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = createQueueResponse.QueueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 5
            }, cancellationToken);

            var success = receiveMessageResponse.Messages
                .Any(x =>
                {
                    var baseMessage = BaseMessage.Parse<BaseMessage>(x.Body);
                    if (baseMessage == null) throw new ApplicationException($"AwsTopicHealthCheck: message body could not be serialized into BaseMessage ({x.MessageId}): {x.Body}");
                    var notification = AwsNotification.Parse(x.Body);
                    if (notification == null || string.IsNullOrWhiteSpace(notification.Message)) throw new ApplicationException("AwsTopicHealthCheck: Error: could not convert Notification to AwsNotification object");
                    return JsonSerializer.Deserialize<BaseMessage>(notification.Message)?.EventName == testMessageName;
                });

            foreach (var x in receiveMessageResponse.Messages)
            {
                await sqsClient.DeleteMessageAsync(createQueueResponse.QueueUrl, x.ReceiptHandle, cancellationToken);
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
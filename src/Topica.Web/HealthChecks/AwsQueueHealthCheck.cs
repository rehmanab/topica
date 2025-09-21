using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Topica.Messages;

namespace Topica.Web.HealthChecks;

public class AwsQueueHealthCheck(IAmazonSQS sqsClient, IWebHostEnvironment env) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var queueName = $"topica_aws_queue_health_check_web_{env.EnvironmentName.ToLower()}";

        var sw = Stopwatch.StartNew();

        try
        {
            var createQueueResponse = await sqsClient.CreateQueueAsync(queueName, cancellationToken);

            Uri.TryCreate(createQueueResponse?.QueueUrl, UriKind.Absolute, out var queueUri);

            if (createQueueResponse is null || createQueueResponse.HttpStatusCode != HttpStatusCode.OK || string.IsNullOrWhiteSpace(queueUri?.ToString()))
            {
                return HealthCheckResult.Unhealthy("Failed to create or retrieve Queue URL.", data: new Dictionary<string, object>
                {
                    { "QueueName", queueName }
                });
            }

            var testMessageName = Guid.NewGuid().ToString();

            var sendMessageResponse = await sqsClient.SendMessageAsync(queueUri.AbsoluteUri, JsonSerializer.Serialize(new BaseMessage
            {
                ConversationId = Guid.NewGuid(),
                EventId = 1,
                EventName = testMessageName,
                Type = nameof(BaseMessage),
                MessageGroupId = Guid.NewGuid().ToString()
            }), cancellationToken);

            if (sendMessageResponse is null || sendMessageResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                return HealthCheckResult.Unhealthy("Failed to send message to Queue.", data: new Dictionary<string, object>
                {
                    { "QueueName", queueName }
                });
            }

            var receiveMessageResponse = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = queueUri.AbsoluteUri,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 5
            }, cancellationToken);

            var success = receiveMessageResponse.Messages.Any(x => JsonSerializer.Deserialize<BaseMessage>(x.Body)?.EventName == testMessageName);

            foreach (var x in receiveMessageResponse.Messages)
            {
                await sqsClient.DeleteMessageAsync(queueUri.AbsoluteUri, x.ReceiptHandle, cancellationToken);
            }

            return success
                ? HealthCheckResult.Healthy($"Publish to host {queueUri.Host} - Success", data: new Dictionary<string, object>
                {
                    { "SendQueueName", queueName }
                })
                : HealthCheckResult.Unhealthy($"Failed Queue health - did not receive message on host: {queueUri.Host}", data: new Dictionary<string, object>
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
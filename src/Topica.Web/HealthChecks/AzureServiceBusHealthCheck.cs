using System.Diagnostics;
using System.Text;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Topica.Azure.ServiceBus.Contracts;
using Topica.Helpers;
using Topica.Messages;

namespace Topica.Web.HealthChecks;

public class AzureServiceBusHealthCheck(IAzureServiceBusAdministrationClientProvider administrationClientProvider, IAzureServiceBusClientProvider azureServiceBusClientProvider, IWebHostEnvironment env) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        var topicName = $"topica_azure_sb_topic_health_check_web_topic_{env.EnvironmentName.ToLower()}";
        var subscribedQueueName = $"topica_azure_sb_topic_health_check_web_queue_{env.EnvironmentName.ToLower()}";

        var sw = Stopwatch.StartNew();

        try
        {
            var connectionStringEndpoint = CloudConnectionStringHelper.ParseEndpointCloudConnectionString(azureServiceBusClientProvider.ConnectionString);
            if (!string.IsNullOrWhiteSpace(connectionStringEndpoint) && connectionStringEndpoint.Contains(azureServiceBusClientProvider.DomainUrlSuffix))
            {
                var topicOptions = new CreateTopicOptions(topicName)
                {
                    AutoDeleteOnIdle = TimeSpan.MaxValue, // Default
                    DefaultMessageTimeToLive = TimeSpan.MaxValue, // Default
                    DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(1), // Default
                    EnableBatchedOperations = true, // Default
                    EnablePartitioning = false, // Default
                    MaxSizeInMegabytes = 1024, // Default
                    RequiresDuplicateDetection = true,
                    UserMetadata = "", // Default
                    MaxMessageSizeInKilobytes = 256, // Default - 256 KB (standard tier) or 100 MB (premium tier)
                    Status = EntityStatus.Active, // Default
                    SupportOrdering = false // Default
                };

                topicOptions.AuthorizationRules.Add(new SharedAccessAuthorizationRule("allClaims", [AccessRights.Manage, AccessRights.Send, AccessRights.Listen]));

                if (!await administrationClientProvider.AdminClient.TopicExistsAsync(topicName, cancellationToken))
                {
                    await administrationClientProvider.AdminClient.CreateTopicAsync(topicOptions, cancellationToken);
                }

                if (!await administrationClientProvider.AdminClient.SubscriptionExistsAsync(topicName, subscribedQueueName, cancellationToken))
                {
                    var subscriptionOptions = new CreateSubscriptionOptions(topicName, subscribedQueueName)
                    {
                        AutoDeleteOnIdle = TimeSpan.MaxValue, // Default
                        DefaultMessageTimeToLive = TimeSpan.MaxValue, // Default
                        EnableBatchedOperations = true, // Default
                        UserMetadata = "",
                        DeadLetteringOnMessageExpiration = false, // Default
                        EnableDeadLetteringOnFilterEvaluationExceptions = true, // Default
                        ForwardDeadLetteredMessagesTo = null, // Default
                        ForwardTo = null, // Default
                        LockDuration = TimeSpan.FromSeconds(60), // Default
                        MaxDeliveryCount = 10, // Default
                        RequiresSession = false, // Default
                        Status = EntityStatus.Active, // Default
                    };

                    _ = await administrationClientProvider.AdminClient.CreateSubscriptionAsync(subscriptionOptions, cancellationToken);
                }
            }

            await using var sender = azureServiceBusClientProvider.Client.CreateSender(topicName, new ServiceBusSenderOptions { Identifier = "AzureServiceBusHealthCheckSender" });

            var testMessageName = Guid.NewGuid().ToString();

            var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new BaseMessage
            {
                ConversationId = Guid.NewGuid(),
                EventId = 1,
                EventName = testMessageName,
                Type = nameof(BaseMessage),
            })))
            {
                MessageId = Guid.NewGuid().ToString() // MessageId is or can be used for deduplication
            };

            await sender.SendMessageAsync(serviceBusMessage, cancellationToken);

            await  using var receiver = azureServiceBusClientProvider.Client.CreateReceiver(topicName, subscribedQueueName, new ServiceBusReceiverOptions
            {
                Identifier = "AzureServiceBusHealthCheckReceiver",
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });

            var message = await receiver.ReceiveMessageAsync(cancellationToken: cancellationToken, maxWaitTime: TimeSpan.FromSeconds(20));

            var receivedMessage = JsonConvert.DeserializeObject<BaseMessage>(Encoding.UTF8.GetString(message.Body));

            var success = receivedMessage != null && receivedMessage.EventName == testMessageName;

            await receiver.CompleteMessageAsync(message, cancellationToken);
            
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
}
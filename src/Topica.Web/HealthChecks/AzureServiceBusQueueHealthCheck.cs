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

public class AzureServiceBusQueueHealthCheck(IAzureServiceBusAdministrationClientProvider administrationClientProvider, IAzureServiceBusClientProvider azureServiceBusClientProvider, IWebHostEnvironment env) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        var queueName = $"queue_health_check_web_queue_{env.EnvironmentName.ToLower()}";

        var sw = Stopwatch.StartNew();

        try
        {
            var connectionStringEndpoint = CloudConnectionStringHelper.ParseEndpointCloudConnectionString(azureServiceBusClientProvider.ConnectionString);
            if (!string.IsNullOrWhiteSpace(connectionStringEndpoint) && connectionStringEndpoint.Contains(azureServiceBusClientProvider.DomainUrlSuffix))
            {
                var queueOptions = new CreateQueueOptions(queueName)
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
                };

                queueOptions.AuthorizationRules.Add(new SharedAccessAuthorizationRule("allClaims", [AccessRights.Manage, AccessRights.Send, AccessRights.Listen]));

                if (!await administrationClientProvider.AdminClient.QueueExistsAsync(queueName, cancellationToken))
                {
                    await administrationClientProvider.AdminClient.CreateQueueAsync(queueOptions, cancellationToken);
                }
            }

            await using var sender = azureServiceBusClientProvider.Client.CreateSender(queueName, new ServiceBusSenderOptions { Identifier = "AzureServiceBusQueueHealthCheckSender" });

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

            await  using var receiver = azureServiceBusClientProvider.Client.CreateReceiver(queueName, new ServiceBusReceiverOptions
            {
                Identifier = "AzureServiceBusQueueHealthCheckReceiver",
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });

            var message = await receiver.ReceiveMessageAsync(cancellationToken: cancellationToken, maxWaitTime: TimeSpan.FromSeconds(20));

            var receivedMessage = JsonConvert.DeserializeObject<BaseMessage>(Encoding.UTF8.GetString(message.Body));

            var success = receivedMessage != null && receivedMessage.EventName == testMessageName;

            await receiver.CompleteMessageAsync(message, cancellationToken);
            
            return success
                ? HealthCheckResult.Healthy("Published, Received from Queue: Success", data: new Dictionary<string, object>
                {
                    { "SendQueueName", queueName }
                })
                : HealthCheckResult.Unhealthy("Failed Queue health - did not receive message", data: new Dictionary<string, object>
                {
                    { "QueueName", queueName },
                });
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
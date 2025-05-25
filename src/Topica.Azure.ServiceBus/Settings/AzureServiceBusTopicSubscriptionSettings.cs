namespace Topica.Azure.ServiceBus.Settings;

public class AzureServiceBusTopicSubscriptionSettings
{
    public string? Source { get; set; }
    public string? AutoDeleteOnIdle { get; set; }
    public string? DefaultMessageTimeToLive { get; set; }
    public bool? EnableBatchedOperations { get; set; }
    public string? UserMetadata { get; set; }
    public bool? DeadLetteringOnMessageExpiration { get; set; }
    public bool? EnableDeadLetteringOnFilterEvaluationExceptions { get; set; }
    public string? ForwardDeadLetteredMessagesTo { get; set; }
    public string? ForwardTo { get; set; }
    public string? LockDuration { get; set; }
    public int? MaxDeliveryCount { get; set; }
    public bool? RequiresSession { get; set; }
    public bool? EnabledStatus { get; set; }
}
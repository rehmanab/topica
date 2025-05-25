namespace Topica.Azure.ServiceBus.Settings;

public class AzureServiceBusConsumerSettings
{
    public const string SectionName = nameof(AzureServiceBusConsumerSettings);
    
    public AzureServiceBusConsumerTopicSettings? PriceSubmittedTopicSettings { get; set; }
    public AzureServiceBusConsumerTopicSettings? QuantityUpdatedTopicSettings { get; set; }
}

public class AzureServiceBusConsumerTopicSettings
{
    public string? Source { get; set; }
    public AzureServiceBusTopicSubscriptionSettings[]? Subscriptions { get; set; }
    public string? SubscribeToSource { get; set; }
    public int NumberOfInstances { get; set; } = 1;
    
    public string? AutoDeleteOnIdle { get; set; }
    public string? DefaultMessageTimeToLive { get; set; }
    public string? DuplicateDetectionHistoryTimeWindow { get; set; }
    public bool? EnableBatchedOperations { get; set; }
    public bool? EnablePartitioning { get; set; }
    public int? MaxSizeInMegabytes { get; set; }
    public bool? RequiresDuplicateDetection { get; set; }
    public string? UserMetadata { get; set; }
    public int? MaxMessageSizeInKilobytes { get; set; }
    public bool? EnabledStatus { get; set; }
    public bool? SupportOrdering { get; set; }
}
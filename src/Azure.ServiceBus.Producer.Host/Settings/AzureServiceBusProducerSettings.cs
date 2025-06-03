using Topica.Settings;

namespace Azure.ServiceBus.Producer.Host.Settings;

public class AzureServiceBusProducerSettings
{
    public const string SectionName = nameof(AzureServiceBusProducerSettings);

    public AzureServiceBusTopicSettings WebAnalyticsTopicSettings { get; init; } = null!;
}

public class AzureServiceBusTopicSettings
{
    public string WorkerName { get; set; } = null!;
    public string Source { get; set; } = null!;
    public AzureServiceBusTopicSubscriptionSettings[] Subscriptions { get; set; } = null!;
    public string SubscribeToSource { get; set; } = null!;
    public int? NumberOfInstances { get; set; }
    
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
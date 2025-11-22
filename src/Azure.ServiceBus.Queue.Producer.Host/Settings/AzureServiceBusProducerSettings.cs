namespace Azure.ServiceBus.Queue.Producer.Host.Settings;

public class AzureServiceBusProducerSettings
{
    public const string SectionName = nameof(AzureServiceBusProducerSettings);

    public AzureServiceBusQueueSettings WebAnalyticsQueueSettings { get; init; } = null!;
}

public class AzureServiceBusQueueSettings
{
    public string WorkerName { get; set; } = null!;
    public string Source { get; set; } = null!;
    
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
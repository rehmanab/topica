namespace Pulsar.Consumer.Host.Settings;

public class PulsarConsumerSettings
{
    public static string SectionName => nameof(PulsarConsumerSettings);
    
    public PulsarTopicSettings DataSentTopicSettings { get; init; } = null!;
    public PulsarTopicSettings MatchStartedTopicSettings { get; init; } = null!;
}

public class PulsarTopicSettings
{
    public string Source { get; set; } = null!;
    public string Tenant { get; set; } = null!;
    public string Namespace { get; set; } = null!;

    // Each unique name will start cursor at earliest or latest, then read from that position
    // will read all un-acknowledge per consumer group (actually pulsar uses consumer name).
    // i.e. each consumer name is an independent subscription and acknowledges messages per subscription
    public string ConsumerGroup { get; set; } = null!;

    // Sets any NEW consumers only to the earliest cursor position (can't be changed for existing subscription)
    public bool? StartNewConsumerEarliest { get; set; }

    // Default is to create a partitioned queue to allow concurrent consumers, this requires multiple topic partitions
    public int? NumberOfPartitions { get; set; }
    
    public int? NumberOfInstances { get; set; }
}
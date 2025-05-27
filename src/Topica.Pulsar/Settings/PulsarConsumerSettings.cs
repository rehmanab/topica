namespace Topica.Pulsar.Settings;

public class PulsarConsumerSettings
{
    public static string SectionName => nameof(PulsarConsumerSettings);
    
    public PulsarConsumerTopicSettings? DataSentTopicSettings { get; set; }
    public PulsarConsumerTopicSettings? MatchStartedTopicSettings { get; set; }
}

public class PulsarConsumerTopicSettings
{
    public string? Source { get; } = null!;
    public int NumberOfInstances { get; set; } = 1;
    
    public string? Tenant { get; set; }
    public string? Namespace { get; set; }
    // Each unique name will start cursor at earliest or latest, then read from that position
    // will read all un-acknowledge per consumer group (actually pulsar uses consumer name).
    // i.e. each consumer name is an independent subscription and acknowledges messages per subscription
    public string? ConsumerGroup { get; set; }
    // Sets any NEW consumers only to the earliest cursor position (can't be changed for existing subscription)
    public bool StartNewConsumerEarliest { get; set; }
}
namespace Kafka.Consumer.Host.Settings;

public class KafkaConsumerSettings
{
    public static string SectionName => nameof(KafkaConsumerSettings);
    
    public KafkaConsumerTopicSettings? PersonCreatedTopicSettings { get; set; }
    public KafkaConsumerTopicSettings? PlaceCreatedTopicSettings { get; set; }
}

public class KafkaConsumerTopicSettings
{
    public string? Source { get; set; }
    public int NumberOfInstances { get; set; } = 1;
    
    public string? ConsumerGroup { get; set; }
    public bool StartFromEarliestMessages { get; set; }
    public int NumberOfTopicPartitions { get; set; }
    public string[]? BootstrapServers { get; set; }
}
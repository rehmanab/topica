namespace Kafka.Topic.Consumer.Host.Settings;

public class KafkaConsumerSettings
{
    public static string SectionName => nameof(KafkaConsumerSettings);

    public KafkaTopicSettings WebAnalyticsTopicSettings { get; init; } = null!;
}

public class KafkaTopicSettings
{
    public string WorkerName { get; set; } = null!;
    public string Source { get; set; } = null!;
    public string ConsumerGroup { get; set; } = null!;
    public bool? StartFromEarliestMessages { get; set; }
    public int? NumberOfTopicPartitions { get; set; }
    public int? NumberOfInstances { get; set; }
}
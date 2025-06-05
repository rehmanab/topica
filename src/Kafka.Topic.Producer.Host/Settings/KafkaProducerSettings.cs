namespace Kafka.Topic.Producer.Host.Settings;

public class KafkaProducerSettings
{
    public static string SectionName => nameof(KafkaProducerSettings);

    public KafkaTopicSettings WebAnalyticsTopicSettings { get; init; } = null!;
}

public class KafkaTopicSettings
{
    public string WorkerName { get; set; } = null!;
    public string Source { get; set; } = null!;
    public string ConsumerGroup { get; set; } = null!;
    public bool? StartFromEarliestMessages { get; set; }
    public int? NumberOfTopicPartitions { get; set; }
}
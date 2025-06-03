namespace Kafka.Producer.Host.Settings;

public class KafkaProducerSettings
{
    public static string SectionName => nameof(KafkaProducerSettings);

    public KafkaTopicSettings WebAnalyticsTopicSettings { get; set; } = null!;
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
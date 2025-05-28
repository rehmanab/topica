namespace Kafka.Consumer.Host.Settings;

public class KafkaConsumerSettings
{
    public static string SectionName => nameof(KafkaConsumerSettings);

    public KafkaTopicSettings PersonCreatedTopicSettings { get; set; } = null!;
    public KafkaTopicSettings PlaceCreatedTopicSettings { get; set; } = null!;
}

public class KafkaTopicSettings
{
    public string Source { get; set; } = null!;
    public string ConsumerGroup { get; set; } = null!;
    public bool? StartFromEarliestMessages { get; set; }
    public int? NumberOfTopicPartitions { get; set; }
    public int? NumberOfInstances { get; set; }
}
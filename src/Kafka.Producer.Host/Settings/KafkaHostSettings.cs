namespace Kafka.Producer.Host.Settings;

public class KafkaHostSettings
{
    public static string SectionName => nameof(KafkaHostSettings);

    public string[] BootstrapServers { get; set; } = null!;
}
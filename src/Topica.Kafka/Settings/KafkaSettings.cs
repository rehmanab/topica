namespace Topica.Kafka.Settings
{
    public class KafkaSettings
    {
        public static string SectionName => nameof (KafkaSettings);
        public string[] BootstrapServers { get; set; }
    }
}
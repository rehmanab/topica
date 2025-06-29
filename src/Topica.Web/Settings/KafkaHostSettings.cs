namespace Topica.Web.Settings;

public class KafkaHostSettings
{
    public static string SectionName => nameof(KafkaHostSettings);

    public string[] BootstrapServers { get; init; } = null!;
}
namespace Pulsar.Consumer.Host.Settings;

public class PulsarHostSettings
{
    public static string SectionName => nameof(PulsarHostSettings);

    public string ServiceUrl { get; set; }
    public string PulsarManagerBaseUrl { get; set; }
    public string PulsarAdminBaseUrl { get; set; }
}
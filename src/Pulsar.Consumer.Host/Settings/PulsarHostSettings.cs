namespace Pulsar.Consumer.Host.Settings;

public class PulsarHostSettings
{
    public static string SectionName => nameof(PulsarHostSettings);

    public string ServiceUrl { get; init; } = null!;
    public string PulsarManagerBaseUrl { get; init; } = null!;
    public string PulsarAdminBaseUrl { get; init; } = null!;
}
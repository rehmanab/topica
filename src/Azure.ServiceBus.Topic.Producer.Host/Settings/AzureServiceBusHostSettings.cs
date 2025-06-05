namespace Azure.ServiceBus.Topic.Producer.Host.Settings;

public class AzureServiceBusHostSettings
{
    public const string SectionName = nameof(AzureServiceBusHostSettings);

    public string ConnectionString { get; init; } = null!;
}
namespace Azure.ServiceBus.Topic.Consumer.Host.Settings;

public class AzureServiceBusHostSettings
{
    public const string SectionName = nameof(AzureServiceBusHostSettings);

    public string ConnectionString { get; init; } = null!;
}
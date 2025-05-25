namespace Topica.Azure.ServiceBus.Settings;

public class AzureServiceBusHostSettings
{
    public const string SectionName = nameof(AzureServiceBusHostSettings);

    public string? ConnectionString { get; set; }
}
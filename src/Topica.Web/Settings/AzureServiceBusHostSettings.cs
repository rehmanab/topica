namespace Topica.Web.Settings;

public class AzureServiceBusHostSettings
{
    public const string SectionName = nameof(AzureServiceBusHostSettings);

    public string ConnectionString { get; init; } = null!;
}
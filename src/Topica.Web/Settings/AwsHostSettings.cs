namespace Topica.Web.Settings;

public class AwsHostSettings
{
    public static string SectionName => nameof(AwsHostSettings);

    public string? ProfileName { get; init; }
    public string? AccessKey { get; init; }
    public string? SecretKey { get; init; }
    public string? ServiceUrl { get; init; }
    public string? RegionEndpoint { get; init; }
}
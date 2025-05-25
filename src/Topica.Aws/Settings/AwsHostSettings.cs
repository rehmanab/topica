namespace Topica.Aws.Settings;

public class AwsHostSettings
{
    public static string SectionName => nameof (AwsHostSettings);
    public string? ProfileName { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public string? ServiceUrl { get; set; }
    public string? RegionEndpoint { get; set; }
}
namespace Topica.Aws.Settings;

public class AwsSettings
{
    public static string SectionName => nameof (AwsSettings);
    public string? ProfileName { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public string? ServiceUrl { get; set; }
    public string? RegionEndpoint { get; set; }
}
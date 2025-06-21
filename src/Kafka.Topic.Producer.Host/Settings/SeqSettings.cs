namespace Kafka.Topic.Producer.Host.Settings;

public class SeqSettings
{
    public static string SectionName => nameof(SeqSettings);
    
    public string? ServerUrl { get; set; }
    public string? ApiKey { get; set; }
    public string? MinimumLevel { get; set; }
    public Dictionary<string, string>? LevelOverride { get; set; }
}


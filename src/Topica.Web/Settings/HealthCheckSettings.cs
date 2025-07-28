namespace Topica.Web.Settings;

public class HealthCheckSettings
{
    public static string SectionName => nameof(HealthCheckSettings);
    
    public HealthCheckEnabled HealthCheckEnabled { get; set; } = null!;
}

public class HealthCheckEnabled
{
    public bool AwsQueue { get; set; }   
    public bool AwsTopic { get; set; }   
    public bool AzureServiceBus { get; set; }   
    public bool Kafka { get; set; }   
    public bool Pulsar { get; set; }   
    public bool RabbitMqQueue { get; set; }   
    public bool RabbitMqTopic { get; set; }   
}
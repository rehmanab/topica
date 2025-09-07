using Topica.Web.Models;

namespace Topica.Web.Settings;

public class HealthCheckSettings
{
    public static string SectionName => nameof(HealthCheckSettings);

    public HealthCheckModel AwsQueue { get; set; } = null!;
    public HealthCheckModel AwsTopic { get; set; } = null!;
    public HealthCheckModel AzureServiceBus { get; set; } = null!;
    public HealthCheckModel Kafka { get; set; } = null!;
    public HealthCheckModel Pulsar { get; set; } = null!;
    public HealthCheckModel RabbitMqQueue { get; set; } = null!; 
    public HealthCheckModel RabbitMqTopic { get; set; } = null!;
}

public class HealthCheckModel
{
    public string Name { get; set; } = null!;
    public HealthCheckTag Tag { get; set; }
    public bool Enabled { get; set; }
    public TimeSpan TimeOut { get; set; }
}
namespace RabbitMq.Queue.Consumer.Host.Settings;

public class RabbitMqConsumerSettings
{
    public static string SectionName => nameof(RabbitMqConsumerSettings);

    public RabbitMqConsumerQueueSettings WebAnalyticsQueueSettings { get; init; } = null!;
}

public class RabbitMqConsumerQueueSettings
{
    public string WorkerName { get; set; } = null!;
    public string Source { get; set; } = null!;
    public int? NumberOfInstances { get; set; }
}
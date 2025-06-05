namespace RabbitMq.Queue.Producer.Host.Settings;

public class RabbitMqProducerSettings
{
    public static string SectionName => nameof(RabbitMqProducerSettings);

    public RabbitMqConsumerTopicSettings WebAnalyticsTopicSettings { get; init; } = null!;
}

public class RabbitMqConsumerTopicSettings
{
    public string WorkerName { get; set; } = null!;
    public string Source { get; set; } = null!;
    
    
}
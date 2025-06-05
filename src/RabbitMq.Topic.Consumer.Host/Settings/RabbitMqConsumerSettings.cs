namespace RabbitMq.Topic.Consumer.Host.Settings;

public class RabbitMqConsumerSettings
{
    public static string SectionName => nameof(RabbitMqConsumerSettings);

    public RabbitMqConsumerTopicSettings WebAnalyticsTopicSettings { get; init; } = null!;
}

public class RabbitMqConsumerTopicSettings
{
    public string WorkerName { get; set; } = null!;
    public string Source { get; set; } = null!;
    public string SubscribeToSource { get; set; } = null!;
    public string[] WithSubscribedQueues { get; set; } = null!;
    public int? NumberOfInstances { get; set; }
    
    
}
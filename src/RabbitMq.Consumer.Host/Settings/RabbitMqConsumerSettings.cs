namespace RabbitMq.Consumer.Host.Settings;

public class RabbitMqConsumerSettings
{
    public static string SectionName => nameof(RabbitMqConsumerSettings);

    public RabbitMqConsumerTopicSettings? ItemDeliveredTopicSettings { get; set; }
    public RabbitMqConsumerTopicSettings? ItemPostedTopicSettings { get; set; }
}

public class RabbitMqConsumerTopicSettings
{
    public string? Source { get; set; }
    public string? SubscribeToSource { get; set; }
    public string[]? WithSubscribedQueues { get; set; }
    public int NumberOfInstances { get; set; } = 1;
    
    
}
namespace RabbitMq.Producer.Host.Settings;

public class RabbitMqProducerSettings
{
    public static string SectionName => nameof(RabbitMqProducerSettings);

    public RabbitMqConsumerTopicSettings ItemDeliveredTopicSettings { get; set; } = null!;
    public RabbitMqConsumerTopicSettings ItemPostedTopicSettings { get; set; } = null!;
}

public class RabbitMqConsumerTopicSettings
{
    public string Source { get; set; } = null!;
    public string SubscribeToSource { get; set; } = null!;
    public string[] WithSubscribedQueues { get; set; } = null!;
    public int? NumberOfInstances { get; set; }
    
    
}
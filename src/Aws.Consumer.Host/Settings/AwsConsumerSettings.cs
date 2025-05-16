namespace Aws.Consumer.Host.Settings;

public class AwsConsumerSettings
{
    public static string SectionName => nameof(AwsConsumerSettings);

    public AwsConsumerTopicSettings? OrderPlacedTopicSettings { get; set; }
    public AwsConsumerTopicSettings? CustomerCreatedTopicSettings { get; set; }
}

public class AwsConsumerTopicSettings
{
    public string? Source { get; set; }
    public string? SubscribeToSource { get; set; }
    public string[]? WithSubscribedQueues { get; set; }
    public int NumberOfInstances { get; set; } = 1;

    // Aws
    public bool BuildWithErrorQueue { get; set; }
    public int? ErrorQueueMaxReceiveCount { get; set; }
    public int? VisibilityTimeout { get; set; }
    public bool IsFifoQueue { get; set; }
    public bool IsFifoContentBasedDeduplication { get; set; }
    public int? MaximumMessageSize { get; set; }
    public int ReceiveMaximumNumberOfMessages { get; set; } = 10;
    public int? MessageRetentionPeriod { get; set; }
    public int? DelaySeconds { get; set; }
    public int? ReceiveMessageWaitTimeSeconds { get; set; }
}
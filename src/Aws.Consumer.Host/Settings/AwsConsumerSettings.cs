namespace Aws.Consumer.Host.Settings;

public class AwsConsumerSettings
{
    public static string SectionName => nameof(AwsConsumerSettings);

    public AwsConsumerTopicSettings OrderPlacedTopicSettings { get; set; }
    public AwsConsumerTopicSettings CustomerCreatedTopicSettings { get; set; }
}

public class AwsConsumerTopicSettings
{
    public string Source { get; set; }
    public string SubscribeToSource { get; set; }
    public string[] WithSubscribedQueues { get; set; }
    public int NumberOfInstances { get; set; } = 1;

    // Aws
    public bool AwsBuildWithErrorQueue { get; set; }
    public int? AwsErrorQueueMaxReceiveCount { get; set; }
    public int? AwsVisibilityTimeout { get; set; }
    public bool AwsIsFifoQueue { get; set; }
    public bool AwsIsFifoContentBasedDeduplication { get; set; }
    public int? AwsMaximumMessageSize { get; set; }
    public int AwsReceiveMaximumNumberOfMessages { get; set; } = 10;
    public int? AwsMessageRetentionPeriod { get; set; }
    public int? AwsDelaySeconds { get; set; }
    public int? AwsReceiveMessageWaitTimeSeconds { get; set; }
}
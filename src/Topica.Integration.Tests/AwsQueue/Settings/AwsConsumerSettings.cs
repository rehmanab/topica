namespace Topica.Integration.Tests.AwsQueue.Settings;

public class AwsConsumerSettings
{
    public static string SectionName => nameof(AwsConsumerSettings);

    public AwsConsumerTopicSettings WebAnalyticsQueueSettings { get; init; } = null!;
}

public class AwsConsumerTopicSettings
{
    public string WorkerName { get; set; } = null!;
    public string Source { get; set; } = null!;
    public int? NumberOfInstances { get; set; }

    public bool? IsFifoQueue { get; set; }
    public bool? IsFifoContentBasedDeduplication { get; set; }

    public bool? BuildWithErrorQueue { get; set; }
    public int? ErrorQueueMaxReceiveCount { get; set; }

    public int? MessageVisibilityTimeoutSeconds { get; set; }
    public int? QueueMessageDelaySeconds { get; set; }
    public int? QueueMessageRetentionPeriodSeconds { get; set; }
    public int? QueueReceiveMessageWaitTimeSeconds { get; set; }

    public int? QueueMaximumMessageSize { get; set; }

    public int? QueueReceiveMaximumNumberOfMessages { get; set; }
}
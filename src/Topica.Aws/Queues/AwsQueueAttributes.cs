using System.Collections.Generic;

namespace Topica.Aws.Queues;

public class AwsQueueAttributes
{
    public const int QueueMessageDelaySecondsMin = 0;
    public const int QueueMessageDelaySecondsMax = 900;
    public const int QueueMaximumMessageSizeMin = 1024;
    public const int QueueMaximumMessageSizeMax = 262144;
    public const int QueueMessageRetentionPeriodMin = 60;
    public const int QueueMessageRetentionPeriodMax = 1209600;
    public const int QueueReceiveMessageWaitTimeSecondsMin = 0;
    public const int QueueReceiveMessageWaitTimeSecondsMax = 20;
    public const int MessageVisibilityTimeoutSecondsDefault = 30;
    public const int MessageVisibilityTimeoutSecondsMin = 0;
    public const int MessageVisibilityTimeoutSecondsMax = 43200;

    public const string AllName = "All";
    public const string QueueMessageDelaySecondsName = "DelaySeconds";
    public const string QueueMaximumMessageSizeName = "MaximumMessageSize";
    public const string QueueMessageRetentionPeriodSecondsName = "MessageRetentionPeriod";
    public const string PolicyName = "Policy";
    public const string QueueReceiveMessageWaitTimeSecondsName = "ReceiveMessageWaitTimeSeconds";
    public const string MessageVisibilityTimeoutName = "VisibilityTimeout";
    public const string RedrivePolicyName = "RedrivePolicy";
    public const string QueueArnName = "QueueArn";
    public const string ApproximateNumberOfMessagesName = "ApproximateNumberOfMessages";
    public const string ApproximateNumberOfMessagesNotVisibleName = "ApproximateNumberOfMessagesNotVisible";
    public const string ApproximateNumberOfMessagesDelayedName = "ApproximateNumberOfMessagesDelayed";
    public const string CreatedTimestampName = "CreatedTimestamp";
    public const string LastModifiedTimestampName = "LastModifiedTimestamp";
    public const string IsFifoQueueName = "FifoQueue";
    public const string FifoContentBasedDeduplicationName = "ContentBasedDeduplication";

    public int? QueueMessageDelaySeconds { get; set; }
    public int? QueueMaximumMessageSize { get; set; }
    public int? QueueMessageRetentionPeriodSeconds { get; set; } = 1209600;
    public string? Policy { get; set; } 
    public int? QueueReceiveMessageWaitTimeSeconds { get; set; }
    public int? MessageVisibilityTimeout { get; set; }
    public bool IsFifoQueue { get; set; }
    public bool IsFifoContentBasedDeduplication { get; set; }

    public Dictionary<string, string?> GetAttributeDictionary()
    {
        Validate();

        var properties = new Dictionary<string, string?>();

        if(QueueMessageDelaySeconds.HasValue) properties.Add(QueueMessageDelaySecondsName, QueueMessageDelaySeconds.Value.ToString());
        if(QueueMaximumMessageSize.HasValue) properties.Add(QueueMaximumMessageSizeName, QueueMaximumMessageSize.Value.ToString());
        if(QueueMessageRetentionPeriodSeconds.HasValue) properties.Add(QueueMessageRetentionPeriodSecondsName, QueueMessageRetentionPeriodSeconds.Value.ToString());
        if(!string.IsNullOrWhiteSpace(Policy)) properties.Add(PolicyName, Policy);
        if(QueueReceiveMessageWaitTimeSeconds.HasValue) properties.Add(QueueReceiveMessageWaitTimeSecondsName, QueueReceiveMessageWaitTimeSeconds.Value.ToString());
        if(MessageVisibilityTimeout.HasValue) properties.Add(MessageVisibilityTimeoutName, MessageVisibilityTimeout.Value.ToString());
        if (IsFifoQueue) properties.Add(IsFifoQueueName, "true");
        if (IsFifoContentBasedDeduplication) properties.Add(FifoContentBasedDeduplicationName, "true");

        return properties;
    }

    private void Validate()
    {
        if (QueueMessageDelaySeconds is < QueueMessageDelaySecondsMin or > QueueMessageDelaySecondsMax) 
            throw new AwsQueueAttributesException($"AWS: DelaySeconds must be between 0 and 300, currently: {QueueMessageDelaySeconds.Value}");

        if (QueueMaximumMessageSize is < QueueMaximumMessageSizeMin or > QueueMaximumMessageSizeMax)
            throw new AwsQueueAttributesException($"AWS: MaximumMessageSize must be between 1024 and 262144, currently: {QueueMaximumMessageSize.Value}");

        if (QueueMessageRetentionPeriodSeconds is < QueueMessageRetentionPeriodMin or > QueueMessageRetentionPeriodMax)
            throw new AwsQueueAttributesException($"AWS: MessageRetentionPeriod must be between 60 and 1209600, currently: {QueueMessageRetentionPeriodSeconds.Value}");

        if (QueueReceiveMessageWaitTimeSeconds is < QueueReceiveMessageWaitTimeSecondsMin or > QueueReceiveMessageWaitTimeSecondsMax)
            throw new AwsQueueAttributesException($"AWS: ReceiveMessageWaitTimeSeconds must be between 0 and 20, currently: {QueueReceiveMessageWaitTimeSeconds.Value}");

        if (MessageVisibilityTimeout is < MessageVisibilityTimeoutSecondsMin or > MessageVisibilityTimeoutSecondsMax)
            throw new AwsQueueAttributesException($"AWS: VisibilityTimeout must be between 0 and 43200, currently: {MessageVisibilityTimeout.Value}");
    }
}
using System.Collections.Generic;
using Aws.Messaging.Exceptions;

namespace Aws.Messaging.Config
{
    public class AwsQueueAttributes
    {
        public const int DelaySecondsMin = 0;
        public const int DelaySecondsMax = 900;
        public const int MaximumMessageSizeMin = 1024;
        public const int MaximumMessageSizeMax = 262144;
        public const int MessageRetentionPeriodMin = 60;
        public const int MessageRetentionPeriodMax = 1209600;
        public const int ReceiveMessageWaitTimeSecondsMin = 0;
        public const int ReceiveMessageWaitTimeSecondsMax = 20;
        public const int VisibilityTimeoutMin = 0;
        public const int VisibilityTimeoutMax = 43200;

        public const string AllName = "All";
        public const string DelaySecondsName = "DelaySeconds";
        public const string MaximumMessageSizeName = "MaximumMessageSize";
        public const string MessageRetentionPeriodName = "MessageRetentionPeriod";
        public const string PolicyName = "Policy";
        public const string ReceiveMessageWaitTimeSecondsName = "ReceiveMessageWaitTimeSeconds";
        public const string VisibilityTimeoutName = "VisibilityTimeout";
        public const string RedrivePolicyName = "RedrivePolicy";
        public const string QueueArnName = "QueueArn";
        public const string ApproximateNumberOfMessagesName = "ApproximateNumberOfMessages";
        public const string ApproximateNumberOfMessagesNotVisibleName = "ApproximateNumberOfMessagesNotVisible";
        public const string ApproximateNumberOfMessagesDelayedName = "ApproximateNumberOfMessagesDelayed";
        public const string CreatedTimestampName = "CreatedTimestamp";
        public const string LastModifiedTimestampName = "LastModifiedTimestamp";
        public const string IsFifoQueueName = "FifoQueue";
        public const string FifoContentBasedDeduplicationName = "ContentBasedDeduplication";

        public int? DelaySeconds { get; set; }
        public int? MaximumMessageSize { get; set; }
        public int? MessageRetentionPeriod { get; set; } = 1209600;
        public string Policy { get; set; } 
        public int? ReceiveMessageWaitTimeSeconds { get; set; }
        public int? VisibilityTimeout { get; set; }
        public string RedrivePolicy { get; set; }
        public bool IsFifoQueue { get; set; }
        public bool IsFifoContentBasedDeduplication { get; set; }

        public Dictionary<string, string> GetAttributeDictionary()
        {
            Validate();

            var properties = new Dictionary<string, string>();

            if(DelaySeconds.HasValue) properties.Add(DelaySecondsName, DelaySeconds.Value.ToString());
            if(MaximumMessageSize.HasValue) properties.Add(MaximumMessageSizeName, MaximumMessageSize.Value.ToString());
            if(MessageRetentionPeriod.HasValue) properties.Add(MessageRetentionPeriodName, MessageRetentionPeriod.Value.ToString());
            if(!string.IsNullOrWhiteSpace(Policy)) properties.Add(PolicyName, Policy);
            if(ReceiveMessageWaitTimeSeconds.HasValue) properties.Add(ReceiveMessageWaitTimeSecondsName, ReceiveMessageWaitTimeSeconds.Value.ToString());
            if(VisibilityTimeout.HasValue) properties.Add(VisibilityTimeoutName, VisibilityTimeout.Value.ToString());
            if(!string.IsNullOrWhiteSpace(RedrivePolicy)) properties.Add(RedrivePolicyName, RedrivePolicy);
            if (IsFifoQueue)
            {
                properties.Add(IsFifoQueueName, "true");
                properties.Add(FifoContentBasedDeduplicationName, IsFifoContentBasedDeduplication.ToString().ToLower());
            }

            return properties;
        }

        private void Validate()
        {
            if (DelaySeconds.HasValue)
            {
                if(DelaySeconds.Value < DelaySecondsMin || DelaySeconds.Value > DelaySecondsMax)
                    throw new AwsQueueAttributesException($"AWS: DelaySeconds must be between 0 and 300, currently: {DelaySeconds.Value}");
            }

            if (MaximumMessageSize.HasValue)
            {
                if (MaximumMessageSize.Value < MaximumMessageSizeMin || MaximumMessageSize.Value > MaximumMessageSizeMax)
                    throw new AwsQueueAttributesException($"AWS: MaximumMessageSize must be between 1024 and 262144, currently: {MaximumMessageSize.Value}");
            }

            if (MessageRetentionPeriod.HasValue)
            {
                if (MessageRetentionPeriod.Value < MessageRetentionPeriodMin || MessageRetentionPeriod.Value > MessageRetentionPeriodMax)
                    throw new AwsQueueAttributesException($"AWS: MessageRetentionPeriod must be between 60 and 1209600, currently: {MessageRetentionPeriod.Value}");
            }

            if (ReceiveMessageWaitTimeSeconds.HasValue)
            {
                if (ReceiveMessageWaitTimeSeconds.Value < ReceiveMessageWaitTimeSecondsMin || ReceiveMessageWaitTimeSeconds.Value > ReceiveMessageWaitTimeSecondsMax)
                    throw new AwsQueueAttributesException($"AWS: ReceiveMessageWaitTimeSeconds must be between 0 and 20, currently: {ReceiveMessageWaitTimeSeconds.Value}");
            }

            if (VisibilityTimeout.HasValue)
            {
                if (VisibilityTimeout.Value < VisibilityTimeoutMin || VisibilityTimeout.Value > VisibilityTimeoutMax)
                    throw new AwsQueueAttributesException($"AWS: VisibilityTimeout must be between 0 and 43200, currently: {VisibilityTimeout.Value}");
            }
        }
    }
}
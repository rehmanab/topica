namespace Topica.Aws.Settings
{
    public class AwsDefaultAttributeSettings
    {
        public int? DelayInSeconds { get; set; }
        public int? MaximumMessageSize { get; set; }
        public int? MessageRetentionPeriod { get; set; }
        public int? ReceiveMessageWaitTimeSeconds { get; set; }
        public int? VisibilityTimeout { get; set; }
        public AwsSqsFifoQueueSettings? FifoSettings { get; set; }
    }

    public class AwsSqsFifoQueueSettings
    {
        public bool? IsFifoQueue { get; set; }
        public bool? IsContentBasedDeduplication { get; set; }
    }
}
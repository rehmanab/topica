namespace Aws.Messaging.Settings
{
    public class AwsDefaultAttributeSettings
    {
        public int DelayInSeconds { get; set; }
        public int MaximumMessageSize { get; set; }
        public int MessageRetentionPeriod { get; set; }
        public int ReceiveMessageWaitTimeSeconds { get; set; }
        public int VisibilityTimeout { get; set; }
        public bool FifoQueue { get; set; } = true;
    }
}
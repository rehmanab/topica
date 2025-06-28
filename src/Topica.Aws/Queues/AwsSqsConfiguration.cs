namespace Topica.Aws.Queues
{
    public class AwsSqsConfiguration
    {
        public AwsQueueAttributes QueueAttributes { get; init; } = new();
        public bool CreateErrorQueue { get; init; }
        public int ErrorQueueMaxReceiveCount { get; init; }
    }
}
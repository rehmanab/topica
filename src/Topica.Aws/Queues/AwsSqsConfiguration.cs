namespace Topica.Aws.Queues
{
    public class AwsSqsConfiguration
    {
        public AwsQueueAttributes QueueAttributes { get; set; } = new();
        public bool? CreateErrorQueue { get; set; }
        public int ErrorQueueMaxReceiveCount { get; set; }
    }
}
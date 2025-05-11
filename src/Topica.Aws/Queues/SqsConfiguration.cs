namespace Topica.Aws.Queues
{
    public class SqsConfiguration
    {
        public AwsQueueAttributes QueueAttributes { get; set; } = new();
        public bool? CreateErrorQueue { get; set; }
        public int? MaxReceiveCount { get; set; }
    }
}
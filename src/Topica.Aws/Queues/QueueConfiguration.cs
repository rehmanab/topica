namespace Topica.Aws.Queues
{
    public class QueueConfiguration
    {
        public QueueConfiguration()
        {
            QueueAttributes = new AwsQueueAttributes();    
        }

        public AwsQueueAttributes QueueAttributes { get; set; }
        public bool? CreateErrorQueue { get; set; }
        public int? MaxReceiveCount { get; set; }
    }
}
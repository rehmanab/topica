namespace Aws.Messaging.Config
{
    public class SqsConfiguration
    {
        public SqsConfiguration()
        {
            QueueAttributes = new AwsQueueAttributes();    
        }

        public AwsQueueAttributes QueueAttributes { get; set; }
        public bool? CreateErrorQueue { get; set; }
        public int? MaxReceiveCount { get; set; }
    }
}
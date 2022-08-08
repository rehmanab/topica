using Topica.Messages;

namespace Topica.Aws.Messages
{
    public class BaseAwsMessage : Message
    {
        public string? Type { get; set; }
        public string? MessageId { get; set; }
        public string ReceiptHandle { get; set; }
        public string? QueueUrl { get; set; }
        public string? TopicArn { get; set; }
    }
}
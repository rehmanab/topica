using System;

namespace Topica.Aws.Messages
{
    public class BaseSqsMessage
    {
        public string? Type { get; set; }
        public string? MessageId { get; set; }
        public Guid ConversationId { get; set; }
        public DateTime TimeStampUtc { get; set; }
        public string? RaisingComponent { get; set; }
        public string? Version { get; set; }
        public string? SourceIp { get; set; }
        public string ReceiptHandle { get; set; }
        public string? QueueUrl { get; set; }
        public string? TopicArn { get; set; }
    }
}
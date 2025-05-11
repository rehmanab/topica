using Topica.Messages;

namespace Aws.Consumer.Host.Messages.V1
{
    public class OrderCreatedMessageV1 : BaseMessage
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
    }
}
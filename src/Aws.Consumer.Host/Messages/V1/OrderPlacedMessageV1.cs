using Topica.Messages;

namespace Aws.Consumer.Host.Messages.V1
{
    public class OrderPlacedMessageV1 : BaseMessage
    {
        public long OrderId { get; set; }
        public string? OrderName { get; set; }
    }
}
using Topica.Messages;

namespace Azure.ServiceBus.Producer.Host.Messages.V1
{
    public class QuantityUpdatedMessageV1 : BaseMessage
    {
        public long QuantityId { get; set; }
        public string? QuantityName { get; set; }
    }
}
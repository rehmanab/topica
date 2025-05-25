using Topica.Messages;

namespace Azure.ServiceBus.Consumer.Host.Messages.V1
{
    public class PriceSubmittedMessageV1 : BaseMessage
    {
        public long PriceId { get; set; }
        public string? PriceName { get; set; }    
    }
}
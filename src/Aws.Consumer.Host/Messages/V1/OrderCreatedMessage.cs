using Topica.Messages;

namespace Aws.Consumer.Host.Messages.V1
{
    public class OrderCreatedMessage : Message
    {
        public string OrderName { get; set; }
    }
}
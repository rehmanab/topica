using Topica.Messages;

namespace Aws.Consumer.Host.Messages
{
    public class OrderCreatedV1 : Message
    {
        public string OrderName { get; set; }
    }
}
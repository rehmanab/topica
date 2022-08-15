using Topica.Messages;

namespace Aws.Consumer.Host.Messages.V1
{
    public class OrderCreatedMessage : Message
    {
        public string Name { get; set; }
    }
}
using Topica.Messages;

namespace Aws.Consumer.Host.Messages.V1
{
    public class CustomerCreatedMessage : Message
    {
        public string Name { get; set; }        
    }
}
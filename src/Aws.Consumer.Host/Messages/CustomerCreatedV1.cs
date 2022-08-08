using Topica.Messages;

namespace Aws.Consumer.Host.Messages
{
    public class CustomerCreatedV1 : Message
    {
        public string CustomerName { get; set; }        
    }
}
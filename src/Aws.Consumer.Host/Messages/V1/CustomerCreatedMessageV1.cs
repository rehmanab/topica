using Topica.Messages;

namespace Aws.Consumer.Host.Messages.V1
{
    public class CustomerCreatedMessageV1 : BaseMessage
    {
        public long CustomerId { get; set; }
        public string CustomerName { get; set; }    
    }
}
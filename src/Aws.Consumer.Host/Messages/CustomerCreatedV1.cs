using Topica.Aws.Messages;

namespace Aws.Consumer.Host.Messages
{
    public class CustomerCreatedV1 : BaseAwsMessage
    {
        public string CustomerName { get; set; }        
    }
}
using Topica.Aws.Messages;
using Topica.Messages;

namespace Aws.Consumer.Host.Messages
{
    public class OrderCreatedV1 : BaseAwsMessage
    {
        public string OrderName { get; set; }
    }
}
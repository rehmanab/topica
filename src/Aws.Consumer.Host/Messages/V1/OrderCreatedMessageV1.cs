using Topica.Messages;

namespace Aws.Consumer.Host.Messages.V1
{
    public class OrderCreatedMessageV1 : BaseMessage
    {
        public string Name { get; set; }
    }
}
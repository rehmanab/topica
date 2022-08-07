using Topica.Aws.Messages;

namespace Aws.Consumer.Host.Messages;

public class CustomerMessage : BaseSqsMessage
{
    public string Name { get; set; }
}
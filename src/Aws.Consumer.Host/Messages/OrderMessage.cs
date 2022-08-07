using Topica.Aws.Messages;

namespace Aws.Consumer.Host.Messages;

public class OrderMessage : BaseSqsMessage
{
    public string Message { get; set; }
}
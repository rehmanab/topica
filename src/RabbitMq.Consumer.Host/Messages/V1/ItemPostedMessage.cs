using Topica.Messages;

namespace RabbitMq.Consumer.Host.Messages.V1;

public class ItemPostedMessage : BaseMessage
{
    public string Name { get; set; }   
}
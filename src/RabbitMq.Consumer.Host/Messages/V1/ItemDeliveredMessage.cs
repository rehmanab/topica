using Topica.Messages;

namespace RabbitMq.Consumer.Host.Messages.V1;

public class ItemDeliveredMessage : BaseMessage
{
    public string? Name { get; set; }   
}
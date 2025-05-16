using Topica.Messages;

namespace RabbitMq.Producer.Host.Messages.V1;

public class ItemDeliveredMessage : BaseMessage
{
    public string? Name { get; set; }   
}
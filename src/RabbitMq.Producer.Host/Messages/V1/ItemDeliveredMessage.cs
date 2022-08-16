using Topica.Messages;

namespace RabbitMq.Producer.Host.Messages.V1;

public class ItemDeliveredMessage : Message
{
    public string Name { get; set; }   
}
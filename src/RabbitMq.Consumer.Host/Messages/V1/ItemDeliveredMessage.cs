using Topica.Messages;

namespace RabbitMq.Consumer.Host.Messages.V1;

public class ItemDeliveredMessage : Message
{
    public bool HandedToResident { get; set; }   
}
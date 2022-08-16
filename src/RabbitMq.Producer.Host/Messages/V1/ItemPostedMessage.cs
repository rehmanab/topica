using Topica.Messages;

namespace RabbitMq.Producer.Host.Messages.V1;

public class ItemPostedMessage : Message
{
    public string Name { get; set; }   
}
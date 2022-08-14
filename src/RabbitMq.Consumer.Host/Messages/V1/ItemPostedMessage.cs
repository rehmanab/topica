using Topica.Messages;

namespace RabbitMq.Consumer.Host.Messages.V1;

public class ItemPostedMessage : Message
{
    public string PostTown { get; set; }   
}
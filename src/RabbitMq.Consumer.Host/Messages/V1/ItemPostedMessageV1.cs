using Topica.Messages;

namespace RabbitMq.Consumer.Host.Messages.V1;

public class ItemPostedMessageV1 : BaseMessage
{
    public long PostboxId { get; set; }
    public string? PostboxName { get; set; }   
}
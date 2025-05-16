using Topica.Messages;

namespace RabbitMq.Consumer.Host.Messages.V1;

public class ItemDeliveredMessageV1 : BaseMessage
{
    public long ItemId { get; set; }
    public string? ItemName { get; set; }   
}
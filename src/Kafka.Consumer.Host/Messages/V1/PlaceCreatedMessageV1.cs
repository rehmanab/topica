using Topica.Messages;

namespace Kafka.Consumer.Host.Messages.V1;

public class PlaceCreatedMessageV1 : BaseMessage
{
    public long PlaceId { get; set; }
    public string? PlaceName { get; set; }
}
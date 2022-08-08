using Topica.Messages;

namespace Kafka.Consumer.Host.Messages;

public class PlaceCreatedV1 : Message
{
    public long Id { get; set; }
    public string PlaceName { get; set; }
}
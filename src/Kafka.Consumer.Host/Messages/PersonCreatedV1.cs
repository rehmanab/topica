using Topica.Messages;

namespace Kafka.Consumer.Host.Messages;

public class PersonCreatedV1 : Message
{
    public long Id { get; set; }
    public string PersonName { get; set; }
}
using Topica.Messages;

namespace Kafka.Consumer.Host.Messages.V1;

public class PersonCreatedMessage : Message
{
    public long Id { get; set; }
    public string Name { get; set; }
}
using Topica.Messages;

namespace Kafka.Consumer.Host.Messages.V1;

public class PersonCreatedMessageV1 : BaseMessage
{
    public long PersonId { get; set; }
    public string? PersonName { get; set; }
}
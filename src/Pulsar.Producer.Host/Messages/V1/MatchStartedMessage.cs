using Topica.Messages;

namespace Pulsar.Producer.Host.Messages.V1;

public class MatchStartedMessage : BaseMessage
{
    public string Name { get; set; }   
}
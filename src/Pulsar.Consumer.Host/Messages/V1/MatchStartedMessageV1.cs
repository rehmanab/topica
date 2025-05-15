using Topica.Messages;

namespace Pulsar.Consumer.Host.Messages.V1;

public class MatchStartedMessageV1 : BaseMessage
{
    public int MatchId { get; set; }   
    public string MatchName { get; set; }   
}
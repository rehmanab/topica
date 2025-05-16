using Topica.Messages;

namespace Pulsar.Producer.Host.Messages.V1;

public class MatchStartedMessageV1 : BaseMessage
{
    public long MatchId { get; set; }
    public string? MatchName { get; set; }   
}
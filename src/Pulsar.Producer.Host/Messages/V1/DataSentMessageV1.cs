using Topica.Messages;

namespace Pulsar.Producer.Host.Messages.V1;

public class DataSentMessageV1 : BaseMessage
{
    public long DataId { get; set; }
    public string? DataName { get; set; }   
}
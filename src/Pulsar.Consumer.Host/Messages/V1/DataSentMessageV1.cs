using Topica.Messages;

namespace Pulsar.Consumer.Host.Messages.V1;

public class DataSentMessageV1 : BaseMessage
{
    public int DataId { get; set; }   
    public string? DataName { get; set; }   
}
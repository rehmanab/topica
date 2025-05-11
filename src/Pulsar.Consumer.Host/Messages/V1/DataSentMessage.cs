using Topica.Messages;

namespace Pulsar.Consumer.Host.Messages.V1;

public class DataSentMessage : BaseMessage
{
    public string Name { get; set; }   
}
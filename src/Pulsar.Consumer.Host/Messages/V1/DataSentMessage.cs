using Topica.Messages;

namespace Pulsar.Consumer.Host.Messages.V1;

public class DataSentMessage : Message
{
    public string Name { get; set; }   
}
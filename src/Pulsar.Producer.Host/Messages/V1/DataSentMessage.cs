using Topica.Messages;

namespace Pulsar.Producer.Host.Messages.V1;

public class DataSentMessage : Message
{
    public string Name { get; set; }   
}
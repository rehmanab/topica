using Aws.Consumer.Host.Handlers;

namespace Aws.Consumer.Host;

public class ConsumerSettings
{
    public static string SectionName => nameof (ConsumerSettings);
    public ConsumerItems[] Consumers { get; set; }
}

public class ConsumerItems
{
    public string Type { get; set; }
    public string QueueName{ get; set; }
    public int NumberOfInstances { get; set; }
}
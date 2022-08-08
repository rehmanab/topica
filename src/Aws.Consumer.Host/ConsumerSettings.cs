namespace Aws.Consumer.Host;

public class ConsumerSettings
{
    public static string SectionName => nameof (ConsumerSettings);
    public ConsumerItem OrderCreated { get; set; }
    public ConsumerItem CustomerCreated { get; set; }
    public int NumberOfInstancesPerConsumer { get; set; }
}

public class ConsumerItem
{
    public string QueueName{ get; set; }
}
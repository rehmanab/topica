namespace Aws.Consumer.Host;

public class WorkerConfiguration
{
    public string OrderQueueName{ get; set; }
    public int DelayMilliseconds { get; set; }
    public int NumberOfInstances { get; set; }
}
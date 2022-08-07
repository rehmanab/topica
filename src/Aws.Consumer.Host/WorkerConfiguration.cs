namespace Aws.Consumer.Host;

public class WorkerConfiguration
{
    public string QueueName{ get; set; }
    public int DelayMilliseconds { get; set; }
    public int NumberOfThreads { get; set; }
}
namespace Topica.Pulsar.Models;

public class PartitionedTopicMetaData
{
    public int Partitions { get; set; }
    public bool Deleted { get; set; }
}
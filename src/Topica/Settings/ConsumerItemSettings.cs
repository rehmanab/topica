namespace Topica.Settings
{
    public class ConsumerItemSettings
    {
        public string Source{ get; set; }
        public string ConsumerGroup { get; set; }
        public bool StartFromEarliestMessages { get; set; }
        public int NumberOfTopicPartitions { get; set; }
        public int NumberOfInstances { get; set; }
    }
}
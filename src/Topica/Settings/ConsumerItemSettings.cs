namespace Topica.Settings
{
    public class ConsumerItemSettings
    {
        public string Source{ get; set; }
        public string ConsumerGroup { get; set; }
        public bool StartFromEarliestMessages { get; set; }
    }
}
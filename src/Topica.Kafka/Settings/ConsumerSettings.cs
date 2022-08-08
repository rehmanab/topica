using Topica.Settings;

namespace Topica.Kafka.Settings
{
    public class ConsumerSettings
    {
        public static string SectionName => nameof (ConsumerSettings);
        public ConsumerItemSettings PlaceCreated { get; set; }
        public ConsumerItemSettings PersonCreated { get; set; }
        public int NumberOfInstancesPerConsumer { get; set; }
    }
}
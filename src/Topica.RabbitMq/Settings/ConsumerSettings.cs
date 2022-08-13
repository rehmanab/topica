using Topica.Settings;

namespace Topica.RabbitMq.Settings
{
    public class ConsumerSettings
    {
        public static string SectionName => nameof (ConsumerSettings);
        public ConsumerItemSettings ItemPosted { get; set; }
        public ConsumerItemSettings ItemDelivered { get; set; }
        public int NumberOfInstancesPerConsumer { get; set; }
    }
}
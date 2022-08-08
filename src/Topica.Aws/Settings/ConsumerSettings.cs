using Topica.Settings;

namespace Topica.Aws.Settings
{
    public class ConsumerSettings
    {
        public static string SectionName => nameof (ConsumerSettings);
        public ConsumerItemSettings OrderCreated { get; set; }
        public ConsumerItemSettings CustomerCreated { get; set; }
        public int NumberOfInstancesPerConsumer { get; set; }
    }
}
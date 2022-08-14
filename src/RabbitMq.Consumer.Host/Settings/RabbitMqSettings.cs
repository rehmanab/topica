namespace RabbitMq.Consumer.Host.Settings
{
    public class RabbitMqSettings
    {
        public static string SectionName => nameof(RabbitMqSettings);

        public string Scheme { get; set; } = null!;
        public string Hostname { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public int Port { get; set; }
        public string VHost { get; set; } = null!;
        public int? ManagementPort { get; set; }
        public string ManagementScheme { get; set; } = null!;
    }
}
namespace Topica.Web.Settings
{
    public class RabbitMqHostSettings
    {
        public static string SectionName => nameof(RabbitMqHostSettings);

        public string Scheme { get; init; } = null!;
        public string Hostname { get; init; } = null!;
        public string UserName { get; init; } = null!;
        public string Password { get; init; } = null!;
        public int Port { get; init; }
        public string VHost { get; init; } = null!;
        public int ManagementPort { get; init; }
        public string ManagementScheme { get; init; } = null!;
    }
}
namespace Topica.RabbitMq.Requests
{
    public class SetUserPermissionsRequest
    {
        public string Username { get; set; }
        public string VHost { get; set; }
        public string Configure { get; set; }
        public string Write { get; set; }
        public string Read { get; set; }
    }
}
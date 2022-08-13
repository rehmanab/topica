namespace Topica.RabbitMq.Requests
{
    public class CreateOrUpdateUserRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Tags { get; set; }
    }
}
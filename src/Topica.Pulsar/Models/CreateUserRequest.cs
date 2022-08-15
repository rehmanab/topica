using Newtonsoft.Json;

namespace Topica.Pulsar.Models
{
    public class CreateUserRequest
    {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("password")] public string Password { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("email")] public string Email { get; set; }
    }
}
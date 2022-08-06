using Newtonsoft.Json;

namespace Topica.Policies
{
    public class RedrivePolicy
    {
        [JsonProperty(PropertyName = "maxReceiveCount")]
        public int MaxReceiveCount { get; set; }

        [JsonProperty(PropertyName = "deadLetterTargetArn")]
        public string DeadLetterTargetArn { get; set; }

        public RedrivePolicy(int maxReceiveCount, string deadLetterTargetArn)
        {
            MaxReceiveCount = maxReceiveCount;
            DeadLetterTargetArn = deadLetterTargetArn;
        }

        public string ToJson() => ToString();

        public override string ToString() => $"{{\"maxReceiveCount\":\"{MaxReceiveCount}\", \"deadLetterTargetArn\":\"{DeadLetterTargetArn}\"}}";

        public static RedrivePolicy ConvertFromString(string policy) => JsonConvert.DeserializeObject<RedrivePolicy>(policy);
    }
}
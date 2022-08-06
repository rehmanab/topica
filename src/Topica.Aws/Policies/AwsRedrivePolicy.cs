using Newtonsoft.Json;

namespace Topica.Aws.Policies
{
    public class AwsRedrivePolicy
    {
        [JsonProperty(PropertyName = "maxReceiveCount")]
        public int MaxReceiveCount { get; set; }

        [JsonProperty(PropertyName = "deadLetterTargetArn")]
        public string DeadLetterTargetArn { get; set; }

        public AwsRedrivePolicy(int maxReceiveCount, string deadLetterTargetArn)
        {
            MaxReceiveCount = maxReceiveCount;
            DeadLetterTargetArn = deadLetterTargetArn;
        }

        public string? ToJson() => ToString();

        public override string? ToString() => $"{{\"maxReceiveCount\":\"{MaxReceiveCount}\", \"deadLetterTargetArn\":\"{DeadLetterTargetArn}\"}}";

        public static AwsRedrivePolicy ConvertFromString(string policy) => JsonConvert.DeserializeObject<AwsRedrivePolicy>(policy)!;
    }
}
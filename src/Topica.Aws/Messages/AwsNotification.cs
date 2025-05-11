using Newtonsoft.Json;

namespace Topica.Aws.Messages;

public class AwsNotification
{
    public string Type { get; set; }
    public string MessageId { get; set; }
    public string TopicArn { get; set; }
    public string Message { get; set; }
    public string Timestamp { get; set; }
    public string UnsubscribeURL { get; set; }
    public MessageAttributes MessageAttributes { get; set; }
    public string SequenceNumber { get; set; }
    
    public static AwsNotification? Parse(string messageBody) 
    {
        return JsonConvert.DeserializeObject<AwsNotification>(messageBody);
    }
}

public class MessageAttributes
{
    public SignatureVersion SignatureVersion { get; set; }
}

public class SignatureVersion
{
    public string Type { get; set; }
    public string Value { get; set; }
}
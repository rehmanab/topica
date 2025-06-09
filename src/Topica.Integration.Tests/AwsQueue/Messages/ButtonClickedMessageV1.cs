using Topica.Messages;

namespace Topica.Integration.Tests.AwsQueue.Messages;

public class ButtonClickedMessageV1 : BaseMessage
{
    public string? ButtonText { get; set; }
    public string? ButtonId { get; set; }
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public DateTime? Timestamp { get; set; }
}
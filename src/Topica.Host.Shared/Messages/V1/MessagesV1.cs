using Topica.Messages;

namespace Topica.Host.Shared.Messages.V1;

public class ButtonClickedMessageV1 : BaseMessage
{
    public string? ButtonText { get; set; }
    public string? ButtonId { get; set; }
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public DateTime? Timestamp { get; set; }
}

public class CookiesAcceptedMessageV1 : BaseMessage
{
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public DateTime? Timestamp { get; set; }
    public bool IsAccepted { get; set; } = false;
}

public class CustomEventMessageV1 : BaseMessage
{
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public DateTime? Timestamp { get; set; }
    public Dictionary<string, string>? EventData { get; set; } = new();
}

public class FileDownloadedMessageV1 : BaseMessage
{
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public DateTime? Timestamp { get; set; }
}

public class LinkClickedMessageV1 : BaseMessage
{
    public string? LinkText { get; set; }
    public string? LinkUrl { get; set; }
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public DateTime? Timestamp { get; set; }
}

public class PageLoadedMessageV1 : BaseMessage
{
    public string? PageName { get; set; }
    public string? PageUrl { get; set; }
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public DateTime? Timestamp { get; set; }
}

public class SearchTriggeredMessageV1 : BaseMessage
{
    public string? SearchTerm { get; set; }   
    public string? SearchType { get; set; }
}

public class UserLoginMessageV1 : BaseMessage
{
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public DateTime? Timestamp { get; set; }
}

public class VideoPlayedMessageV1 : BaseMessage
{
    public string? VideoId { get; set; }
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public DateTime? Timestamp { get; set; }
    public TimeSpan? VideoPosition { get; set; }
}

using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.SharedMessageHandlers.Messages.V1;

namespace Topica.SharedMessageHandlers.Handlers.V1;

public class ButtonClickedMessageHandlerV1(ILogger<ButtonClickedMessageHandlerV1> logger) : IHandler<ButtonClickedMessageV1>
{
    public async Task<bool> HandleAsync(ButtonClickedMessageV1 source, Dictionary<string, string>? properties)
    {
        logger.LogInformation("Handle: {Name} for event: {Data} - {Props}", nameof(ButtonClickedMessageV1), $"{source.EventId} : {source.EventName}", string.Join("; ", properties?.Select(x => $"{x.Key}:{x.Value}") ?? []));
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(ButtonClickedMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}

public class CookiesAcceptedMessageHandlerV1(ILogger<CookiesAcceptedMessageHandlerV1> logger) : IHandler<CookiesAcceptedMessageV1>
{
    public async Task<bool> HandleAsync(CookiesAcceptedMessageV1 source, Dictionary<string, string>? properties)
    {
        logger.LogInformation("Handle: {Name} for event: {Data} - {Props}", nameof(CookiesAcceptedMessageV1), $"{source.EventId} : {source.EventName}", string.Join("; ", properties?.Select(x => $"{x.Key}:{x.Value}") ?? []));
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(CookiesAcceptedMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}

public class CustomEventMessageHandlerV1(ILogger<CustomEventMessageHandlerV1> logger) : IHandler<CustomEventMessageV1>
{
    public async Task<bool> HandleAsync(CustomEventMessageV1 source, Dictionary<string, string>? properties)
    {
        logger.LogInformation("Handle: {Name} for event: {Data} - {Props}", nameof(CustomEventMessageV1), $"{source.EventId} : {source.EventName}", string.Join("; ", properties?.Select(x => $"{x.Key}:{x.Value}") ?? []));
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(CustomEventMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}

public class FileDownloadedMessageHandlerV1(ILogger<FileDownloadedMessageHandlerV1> logger) : IHandler<FileDownloadedMessageV1>
{
    public async Task<bool> HandleAsync(FileDownloadedMessageV1 source, Dictionary<string, string>? properties)
    {
        logger.LogInformation("Handle: {Name} for event: {Data} - {Props}", nameof(FileDownloadedMessageV1), $"{source.EventId} : {source.EventName}", string.Join("; ", properties?.Select(x => $"{x.Key}:{x.Value}") ?? []));
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(FileDownloadedMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}

public class LinkClickedMessageHandlerV1(ILogger<LinkClickedMessageHandlerV1> logger) : IHandler<LinkClickedMessageV1>
{
    public async Task<bool> HandleAsync(LinkClickedMessageV1 source, Dictionary<string, string>? properties)
    {
        logger.LogInformation("Handle: {Name} for event: {Data} - {Props}", nameof(LinkClickedMessageV1), $"{source.EventId} : {source.EventName}", string.Join("; ", properties?.Select(x => $"{x.Key}:{x.Value}") ?? []));
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(LinkClickedMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}

public class PageLoadedMessageHandlerV1(ILogger<PageLoadedMessageHandlerV1> logger) : IHandler<PageLoadedMessageV1>
{
    public async Task<bool> HandleAsync(PageLoadedMessageV1 source, Dictionary<string, string>? properties)
    {
        logger.LogInformation("Handle: {Name} for event: {Data} - {Props}", nameof(PageLoadedMessageV1), $"{source.EventId} : {source.EventName}", string.Join("; ", properties?.Select(x => $"{x.Key}:{x.Value}") ?? []));
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(PageLoadedMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}

public class SearchTriggeredMessageHandlerV1(ILogger<SearchTriggeredMessageHandlerV1> logger) : IHandler<SearchTriggeredMessageV1>
{
    public async Task<bool> HandleAsync(SearchTriggeredMessageV1 source, Dictionary<string, string>? properties)
    {
        logger.LogInformation("Handle: {Name} for event: {Data} - {Props}", nameof(SearchTriggeredMessageV1), $"{source.EventId} : {source.EventName}", string.Join("; ", properties?.Select(x => $"{x.Key}:{x.Value}") ?? []));
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(SearchTriggeredMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}

public class UserLoginMessageHandlerV1(ILogger<UserLoginMessageHandlerV1> logger) : IHandler<UserLoginMessageV1>
{
    public async Task<bool> HandleAsync(UserLoginMessageV1 source, Dictionary<string, string>? properties)
    {
        logger.LogInformation("Handle: {Name} for event: {Data} - {Props}", nameof(UserLoginMessageV1), $"{source.EventId} : {source.EventName}", string.Join("; ", properties?.Select(x => $"{x.Key}:{x.Value}") ?? []));
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(UserLoginMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}

public class VideoPlayedMessageHandlerV1(ILogger<VideoPlayedMessageHandlerV1> logger) : IHandler<VideoPlayedMessageV1>
{
    public async Task<bool> HandleAsync(VideoPlayedMessageV1 source, Dictionary<string, string>? properties)
    {
        logger.LogInformation("Handle: {Name} for event: {Data} - {Props}", nameof(VideoPlayedMessageV1), $"{source.EventId} : {source.EventName}", string.Join("; ", properties?.Select(x => $"{x.Key}:{x.Value}") ?? []));
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(VideoPlayedMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}

public class EvictCacheItemMessageHandlerV1(ILogger<EvictCacheItemMessageHandlerV1> logger) : IHandler<EvictCacheItemMessageV1>
{
    public async Task<bool> HandleAsync(EvictCacheItemMessageV1 source, Dictionary<string, string>? properties)
    {
        logger.LogInformation("Handle: {Name} for event: {Data} - {Props}", nameof(EvictCacheItemMessageV1), $"{source.EventId} : {source.EventName}", string.Join("; ", properties?.Select(x => $"{x.Key}:{x.Value}") ?? []));
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(EvictCacheItemMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}

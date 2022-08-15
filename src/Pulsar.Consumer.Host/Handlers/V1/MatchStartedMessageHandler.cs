using Microsoft.Extensions.Logging;
using Pulsar.Consumer.Host.Messages.V1;
using Topica.Contracts;

namespace Pulsar.Consumer.Host.Handlers.V1;

public class MatchStartedMessageHandler : IHandler<MatchStartedMessage>
{
    private readonly ILogger<MatchStartedMessageHandler> _logger;
        
    public MatchStartedMessageHandler(ILogger<MatchStartedMessageHandler> logger)
    {
        _logger = logger;
    }

    public async Task<bool> HandleAsync(MatchStartedMessage source)
    {
        _logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Match: {PersonName}", nameof(MatchStartedMessage), source.ConversationId, source.Name);
            
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(MatchStartedMessage message)
    {
        return true;
    }
}
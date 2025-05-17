using Microsoft.Extensions.Logging;
using Pulsar.Consumer.Host.Messages.V1;
using Topica.Contracts;

namespace Pulsar.Consumer.Host.Handlers.V1;

public class MatchStartedMessageHandlerV1(ILogger<MatchStartedMessageHandlerV1> logger) : IHandler<MatchStartedMessageV1>
{
    public async Task<bool> HandleAsync(MatchStartedMessageV1 source)
    {
        logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Match: {PersonName}", nameof(MatchStartedMessageV1), source.ConversationId, source.MatchName);
            
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(MatchStartedMessageV1 message)
    {
        if(string.IsNullOrWhiteSpace(message.Type) || !message.Type.Equals(nameof(MatchStartedMessageV1), StringComparison.CurrentCultureIgnoreCase))
        {
            // logger.LogWarning("Invalid message type: {MessageType} for Handler: {Handler}", message.Type, nameof(MatchStartedMessageHandlerV1));
            return false;
        }
        
        return true;
    }
}
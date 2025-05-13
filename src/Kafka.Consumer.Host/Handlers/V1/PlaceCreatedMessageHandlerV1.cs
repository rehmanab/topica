using Kafka.Consumer.Host.Messages.V1;
using Microsoft.Extensions.Logging;
using Topica.Contracts;

namespace Kafka.Consumer.Host.Handlers.V1;

public class PlaceCreatedMessageHandlerV1(ILogger<PlaceCreatedMessageHandlerV1> logger) : IHandler<PlaceCreatedMessageV1>
{
    public async Task<bool> HandleAsync(PlaceCreatedMessageV1 source)
    {
        // logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Place: {PlaceName}", nameof(PlaceCreatedMessageV1), source.ConversationId, source.PlaceName);
            
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(PlaceCreatedMessageV1 messageV1)
    {
        return true;
    }
}
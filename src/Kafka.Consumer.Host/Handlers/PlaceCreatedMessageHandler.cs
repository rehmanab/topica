using Kafka.Consumer.Host.Messages;
using Microsoft.Extensions.Logging;
using Topica.Contracts;

namespace Kafka.Consumer.Host.Handlers;

public class PlaceCreatedMessageHandler : IHandler<PlaceCreatedV1>
{
    private readonly ILogger<PlaceCreatedMessageHandler> _logger;
        
    public PlaceCreatedMessageHandler(ILogger<PlaceCreatedMessageHandler> logger)
    {
        _logger = logger;
    }

    public async Task<bool> HandleAsync(PlaceCreatedV1 source)
    {
        _logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Place: {PlaceName}", nameof(PlaceCreatedV1), source.ConversationId, source.PlaceName);
            
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(PlaceCreatedV1 message)
    {
        return true;
    }
}
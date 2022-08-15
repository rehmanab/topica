using Kafka.Consumer.Host.Messages.V1;
using Microsoft.Extensions.Logging;
using Topica.Contracts;

namespace Kafka.Consumer.Host.Handlers.V1;

public class PlaceCreatedMessageHandler : IHandler<PlaceCreatedMessage>
{
    private readonly ILogger<PlaceCreatedMessageHandler> _logger;
        
    public PlaceCreatedMessageHandler(ILogger<PlaceCreatedMessageHandler> logger)
    {
        _logger = logger;
    }

    public async Task<bool> HandleAsync(PlaceCreatedMessage source)
    {
        _logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Place: {PlaceName}", nameof(PlaceCreatedMessage), source.ConversationId, source.Name);
            
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(PlaceCreatedMessage message)
    {
        return true;
    }
}
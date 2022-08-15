using Kafka.Consumer.Host.Messages.V1;
using Microsoft.Extensions.Logging;
using Topica.Contracts;

namespace Kafka.Consumer.Host.Handlers.V1;

public class PersonCreatedMessageHandler : IHandler<PersonCreatedMessage>
{
    private readonly ILogger<PersonCreatedMessageHandler> _logger;
        
    public PersonCreatedMessageHandler(ILogger<PersonCreatedMessageHandler> logger)
    {
        _logger = logger;
    }

    public async Task<bool> HandleAsync(PersonCreatedMessage source)
    {
        _logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Person: {PersonName}", nameof(PersonCreatedMessage), source.ConversationId, source.Name);
            
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(PersonCreatedMessage message)
    {
        return true;
    }
}
using Kafka.Consumer.Host.Messages;
using Microsoft.Extensions.Logging;
using Topica.Contracts;

namespace Kafka.Consumer.Host.Handlers;

public class PersonCreatedMessageHandler : IHandler<PersonCreatedV1>
{
    private readonly ILogger<PersonCreatedMessageHandler> _logger;
        
    public PersonCreatedMessageHandler(ILogger<PersonCreatedMessageHandler> logger)
    {
        _logger = logger;
    }

    public async Task<bool> HandleAsync(PersonCreatedV1 source)
    {
        _logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Person: {PersonName}", nameof(PersonCreatedV1), source.ConversationId, source.PersonName);
            
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(PersonCreatedV1 message)
    {
        return true;
    }
}
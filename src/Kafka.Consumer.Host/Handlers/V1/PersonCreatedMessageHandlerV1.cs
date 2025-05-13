using Kafka.Consumer.Host.Messages.V1;
using Microsoft.Extensions.Logging;
using Topica.Contracts;

namespace Kafka.Consumer.Host.Handlers.V1;

public class PersonCreatedMessageHandlerV1(ILogger<PersonCreatedMessageHandlerV1> logger) : IHandler<PersonCreatedMessageV1>
{
    public async Task<bool> HandleAsync(PersonCreatedMessageV1 source)
    {
        // logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Person: {PersonName}", nameof(PersonCreatedMessageV1), source.ConversationId, source.PersonName);
            
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(PersonCreatedMessageV1 messageV1)
    {
        return true;
    }
}
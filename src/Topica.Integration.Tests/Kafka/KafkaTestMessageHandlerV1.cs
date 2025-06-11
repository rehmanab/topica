using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Integration.Tests.Shared;
using Topica.Messages;

namespace Topica.Integration.Tests.Kafka;

public class KafkaTestMessageHandlerV1(ILogger<KafkaTestMessageHandlerV1> logger) : IHandler<KafkaTestMessageV1>
{
    public async Task<bool> HandleAsync(KafkaTestMessageV1 source)
    {
        MessageCounter.KafkaTopicMessageReceived.Add(source);
        logger.LogInformation("Handle: {Name} for CID: {ConversationId} for event: {Data}", nameof(KafkaTestMessageV1), source.ConversationId, $"{source.EventId} : {source.EventName}");
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(KafkaTestMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}

public class KafkaTestMessageV1 : BaseMessage;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Integration.Tests.Shared;
using Topica.Messages;

namespace Topica.Integration.Tests.RabbitMq.RabbitMqTopic;

public class RabbitMqTopicTestMessageHandlerV1(ILogger<RabbitMqTopicTestMessageHandlerV1> logger) : IHandler<RabbitMqTopicTestMessageV1>
{
    public async Task<bool> HandleAsync(RabbitMqTopicTestMessageV1 source)
    {
        MessageCounter.RabbitMqTopicMessageReceived.Add(source);
        logger.LogInformation("Handle: {Name} for CID: {ConversationId} for event: {Data}", nameof(RabbitMqTopicTestMessageV1), source.ConversationId, $"{source.EventId} : {source.EventName}");
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(RabbitMqTopicTestMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}

public class RabbitMqTopicTestMessageV1 : BaseMessage;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Integration.Tests.Shared;
using Topica.Messages;

namespace Topica.Integration.Tests.RabbitMq.RabbitMqQueue;

public class RabbitMqQueueTestMessageHandlerV1(ILogger<RabbitMqQueueTestMessageHandlerV1> logger) : IHandler<RabbitMqQueueTestMessageV1>
{
    public async Task<bool> HandleAsync(RabbitMqQueueTestMessageV1 source, Dictionary<string, string>? properties)
    {
        MessageCounter.RabbitMqQueueMessageReceived.Add(new MessageAttributePair{ BaseMessage = source , Attributes = properties});
        logger.LogInformation("Handle: {Name} for event: {Data}", nameof(RabbitMqQueueTestMessageV1), $"{source.EventId} : {source.EventName}");
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(RabbitMqQueueTestMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}

public class RabbitMqQueueTestMessageV1 : BaseMessage;
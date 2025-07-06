using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Integration.Tests.Shared;
using Topica.Messages;

namespace Topica.Integration.Tests.Pulsar;

public class PulsarTestMessageHandlerV1(ILogger<PulsarTestMessageHandlerV1> logger) : IHandler<PulsarTestMessageV1>
{
    public async Task<bool> HandleAsync(PulsarTestMessageV1 source, Dictionary<string, string>? properties)
    {
        MessageCounter.PulsarTopicMessageReceived.Add(new MessageAttributePair{ BaseMessage = source , Attributes = properties});
        logger.LogInformation("Handle: {Name} for event: {Data}", nameof(PulsarTestMessageV1), $"{source.EventId} : {source.EventName}");
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(PulsarTestMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}

public class PulsarTestMessageV1 : BaseMessage;
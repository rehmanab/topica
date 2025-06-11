using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Integration.Tests.Shared;
using Topica.Messages;

namespace Topica.Integration.Tests.Azure.ServiceBus;

public class AzureServiceBusTestMessageHandlerV1(ILogger<AzureServiceBusTestMessageHandlerV1> logger) : IHandler<AzureServiceBusTestMessageV1>
{
    public async Task<bool> HandleAsync(AzureServiceBusTestMessageV1 source)
    {
        MessageCounter.AzureServiceBusTopicMessageReceived.Add(source);
        logger.LogInformation("Handle: {Name} for CID: {ConversationId} for event: {Data}", nameof(AzureServiceBusTestMessageV1), source.ConversationId, $"{source.EventId} : {source.EventName}");
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(AzureServiceBusTestMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}

public class AzureServiceBusTestMessageV1 : BaseMessage;
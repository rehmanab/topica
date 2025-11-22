using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Integration.Tests.Shared;
using Topica.Messages;

namespace Topica.Integration.Tests.Azure.ServiceBusQueue;

public class AzureServiceBusTestMessageHandlerV1(ILogger<AzureServiceBusTestMessageHandlerV1> logger) : IHandler<AzureServiceBusQueueTestMessageV1>
{
    public async Task<bool> HandleAsync(AzureServiceBusQueueTestMessageV1 source, Dictionary<string, string>? properties)
    {
        MessageCounter.AzureServiceBusQueueMessageReceived.Add(new MessageAttributePair{ BaseMessage = source , Attributes = properties});
        logger.LogInformation("Handle: {Name} for event: {Data}", nameof(AzureServiceBusQueueTestMessageV1), $"{source.EventId} : {source.EventName}");
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(AzureServiceBusQueueTestMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}

public class AzureServiceBusQueueTestMessageV1 : BaseMessage;
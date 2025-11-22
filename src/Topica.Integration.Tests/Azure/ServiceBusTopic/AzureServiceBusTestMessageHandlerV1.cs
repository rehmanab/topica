using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Integration.Tests.Shared;
using Topica.Messages;

namespace Topica.Integration.Tests.Azure.ServiceBusTopic;

public class AzureServiceBusTestMessageHandlerV1(ILogger<AzureServiceBusTestMessageHandlerV1> logger) : IHandler<AzureServiceBusTopicTestMessageV1>
{
    public async Task<bool> HandleAsync(AzureServiceBusTopicTestMessageV1 source, Dictionary<string, string>? properties)
    {
        MessageCounter.AzureServiceBusTopicMessageReceived.Add(new MessageAttributePair{ BaseMessage = source , Attributes = properties});
        logger.LogInformation("Handle: {Name} for event: {Data}", nameof(AzureServiceBusTopicTestMessageV1), $"{source.EventId} : {source.EventName}");
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(AzureServiceBusTopicTestMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}

public class AzureServiceBusTopicTestMessageV1 : BaseMessage;
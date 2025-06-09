using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Integration.Tests.AwsQueue.Messages;

namespace Topica.Integration.Tests.AwsQueue.Handlers;

public class ButtonClickedMessageHandlerV1(ILogger<ButtonClickedMessageHandlerV1> logger) : IHandler<ButtonClickedMessageV1>
{
    public async Task<bool> HandleAsync(ButtonClickedMessageV1 source)
    {
        Counter.MessageReceiveCount++;
        logger.LogInformation("Handle: {Name} for CID: {ConversationId} for event: {Data}", nameof(ButtonClickedMessageV1), source.ConversationId, $"{source.EventId} : {source.EventName}");
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(ButtonClickedMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}
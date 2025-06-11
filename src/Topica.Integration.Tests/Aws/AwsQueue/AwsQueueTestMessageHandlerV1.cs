using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Integration.Tests.Shared;
using Topica.Messages;

namespace Topica.Integration.Tests.Aws.AwsQueue;

public class AwsQueueTestMessageHandlerV1(ILogger<AwsQueueTestMessageHandlerV1> logger) : IHandler<AwsQueueTestMessageV1>
{
    public async Task<bool> HandleAsync(AwsQueueTestMessageV1 source)
    {
        MessageCounter.AwsQueueMessageReceived.Add(source);
        logger.LogInformation("Handle: {Name} for CID: {ConversationId} for event: {Data}", nameof(AwsQueueTestMessageV1), source.ConversationId, $"{source.EventId} : {source.EventName}");
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(AwsQueueTestMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}

public class AwsQueueTestMessageV1 : BaseMessage;
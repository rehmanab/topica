using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Integration.Tests.Shared;
using Topica.Messages;

namespace Topica.Integration.Tests.Aws.AwsTopic;

public class AwsTopicTestMessageHandlerV1(ILogger<AwsTopicTestMessageHandlerV1> logger) : IHandler<AwsTopicTestMessageV1>
{
    public async Task<bool> HandleAsync(AwsTopicTestMessageV1 source)
    {
        MessageCounter.AwsTopicMessageReceived.Add(source);
        logger.LogInformation("Handle: {Name} for CID: {ConversationId} for event: {Data}", nameof(AwsTopicTestMessageV1), source.ConversationId, $"{source.EventId} : {source.EventName}");
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(AwsTopicTestMessageV1 message)
    {
        // Todo - Fluent validation for message properties
        return true;
    }
}

public class AwsTopicTestMessageV1 : BaseMessage;
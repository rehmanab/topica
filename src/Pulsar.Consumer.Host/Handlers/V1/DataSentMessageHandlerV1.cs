using Microsoft.Extensions.Logging;
using Pulsar.Consumer.Host.Messages.V1;
using Topica.Contracts;

namespace Pulsar.Consumer.Host.Handlers.V1;

public class DataSentMessageHandlerV1(ILogger<DataSentMessageHandlerV1> logger) : IHandler<DataSentMessageV1>
{
    public async Task<bool> HandleAsync(DataSentMessageV1 source)
    {
        logger.LogInformation("Handle: {Name} for CID: {ConversationId} for Data: {Data}", nameof(DataSentMessageV1), source.ConversationId, $"{source.DataId} : {source.DataName}");
            
        return await Task.FromResult(true);
    }

    public bool ValidateMessage(DataSentMessageV1 message)
    {
        if(string.IsNullOrWhiteSpace(message.Type) || !message.Type.Equals(nameof(DataSentMessageV1), StringComparison.CurrentCultureIgnoreCase))
        {
            // logger.LogWarning("Invalid message type: {MessageType} for Handler: {Handler}", message.Type, nameof(DataSentMessageHandlerV1));
            return false;
        }
        
        return true;
    }
}
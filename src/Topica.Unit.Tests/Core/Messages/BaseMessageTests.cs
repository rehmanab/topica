using Newtonsoft.Json;
using Topica.Messages;

namespace Topica.Unit.Tests.Core.Messages;

public class BaseMessageTests
{
    [Fact]
    public void BaseMessageIdTest()
    {
        var baseMessage = new BaseMessage();
        var id = baseMessage.Id;
        
        // check multiple times to ensure the ID remains consistent
        Assert.Equal(baseMessage.Id.ToString(), id.ToString());
        Assert.Equal(baseMessage.Id.ToString(), id.ToString());
        Assert.Equal(baseMessage.Id.ToString(), id.ToString());
        Assert.Equal(baseMessage.Id.ToString(), id.ToString());
    }

    [Fact]
    public void Parse_ValidMessageBody_ReturnsBaseMessage()
    {
        var originalMessage = new BaseMessage
        {
            Type = "TestType",
            EventId = 12345,
            EventName = "TestEvent",
            ConversationId = Guid.NewGuid(),
            RaisingComponent = "TestComponent",
            Version = "1.0",
            SourceIp = "127.0.0.1",
            Tenant = "TestTenant",
            ReceiptReference = "TestReceipt",
            MessageGroupId = "TestGroup",
            MessageAdditionalProperties = new Dictionary<string, string> { { "Key1", "Value1" } }
        };

        var messageBody = JsonConvert.SerializeObject(originalMessage);
        var parsedMessage = BaseMessage.Parse<BaseMessage>(messageBody);

        Assert.NotNull(parsedMessage);
        Assert.Equal(originalMessage.Type, parsedMessage.Type);
        Assert.Equal(originalMessage.EventId, parsedMessage.EventId);
        Assert.Equal(originalMessage.EventName, parsedMessage.EventName);
        Assert.Equal(originalMessage.ConversationId, parsedMessage.ConversationId);
        Assert.Equal(originalMessage.TimeStampUtc, parsedMessage.TimeStampUtc);
        Assert.Equal(originalMessage.RaisingComponent, parsedMessage.RaisingComponent);
        Assert.Equal(originalMessage.Version, parsedMessage.Version);
        Assert.Equal(originalMessage.SourceIp, parsedMessage.SourceIp);
        Assert.Equal(originalMessage.Tenant, parsedMessage.Tenant);
        Assert.Equal(originalMessage.ReceiptReference, parsedMessage.ReceiptReference);
        Assert.Equal(originalMessage.MessageGroupId, parsedMessage.MessageGroupId);
        Assert.Equal(originalMessage.MessageAdditionalProperties, parsedMessage.MessageAdditionalProperties);
    }

    [Fact]
    public void Parse_InvalidMessageBody_ReturnsNull()
    {
        const string invalidMessageBody = "Invalid JSON";
        var parsedMessage = BaseMessage.Parse<BaseMessage>(invalidMessageBody);

        Assert.Null(parsedMessage);
    }
}
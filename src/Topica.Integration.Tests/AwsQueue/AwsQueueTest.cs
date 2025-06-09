using Topica.Integration.Tests.AwsQueue.Messages;
using Topica.Integration.Tests.CollectionFixtures;
using Xunit;
using Xunit.Abstractions;

namespace Topica.Integration.Tests.AwsQueue;

//// Ensure this test runs in the same collection (group of tests across the project)
/// injecting type T in ICollectionFixture injected once as singleton

[Trait("Category", "Integration"), Collection(nameof(AwsQueueCollection))]
public class AwsQueueTest(SharedFixture sharedFixture, ITestOutputHelper testOutputHelper) 
    : BaseTest(sharedFixture, testOutputHelper)
{
    [Fact]
    public async Task Produce_Consume_Message_Count_Equal()
    {
        Counter.MessageSendCount = 0;
        Counter.MessageReceiveCount = 0;
        
        var producerCts = new CancellationTokenSource();
        var cancelProducerTs = TimeSpan.FromSeconds(5);
        producerCts.CancelAfter(cancelProducerTs);
        
        var consumerCts = new CancellationTokenSource();
        var cancelConsumerTs = TimeSpan.FromSeconds(SharedFixture.DelaySeconds + cancelProducerTs.TotalSeconds);
        consumerCts.CancelAfter(cancelConsumerTs);
        
        Logger.WriteLine("Starting AWS Queue Test, to view ILogging output, please 'Debug' and view debug console.");
        
        await SharedFixture.Consumer.ConsumeAsync(consumerCts.Token);

        try
        {
            await SendSingleAsync(producerCts.Token);
        }
        catch (TaskCanceledException)
        {
            Logger.WriteLine($"Delay for: {SharedFixture.DelaySeconds} secs, to handle all messages.");
            await Task.Delay(TimeSpan.FromSeconds(SharedFixture.DelaySeconds), CancellationToken.None);
        }
        
        Logger.WriteLine($"Messages Sent: {Counter.MessageSendCount}");
                
        Assert.True(Counter.MessageSendCount == Counter.MessageReceiveCount, $"MessageSendCount ({Counter.MessageSendCount}) is not equal to MessageReceiveCount ({Counter.MessageReceiveCount})");
    }
    
    private async Task SendSingleAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var messageGroupId = Guid.NewGuid().ToString();
            
            var message = new ButtonClickedMessageV1
            {
                ConversationId = Guid.NewGuid(), 
                EventId = Counter.MessageSendCount + 1, 
                EventName = "button.clicked.web.v1", 
                Type = nameof(ButtonClickedMessageV1),
                MessageGroupId = messageGroupId
            };

            var messageAttributes = new Dictionary<string, string>
            {
                {"SignatureVersion", "2" }
            };
            
            await SharedFixture.Producer.ProduceAsync(SharedFixture.ProducerQueueName, message, messageAttributes, cancellationToken);
            Counter.MessageSendCount++;

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
    }
}
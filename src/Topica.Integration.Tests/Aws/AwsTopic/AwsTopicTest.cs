using Topica.Integration.Tests.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Topica.Integration.Tests.Aws.AwsTopic;

[Trait("Category", "Integration"), Collection(nameof(AwsTopicCollection))]
public class AwsTopicTest(AwsTopicSharedFixture sharedFixture, ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Produce_Consume_Message_Count_Equal()
    {
        MessageCounter.AwsTopicMessageSent = [];
        MessageCounter.AwsTopicMessageReceived = [];

        var producerCts = new CancellationTokenSource();
        var cancelProducerTs = TimeSpan.FromSeconds(5);
        producerCts.CancelAfter(cancelProducerTs);

        var consumerCts = new CancellationTokenSource();
        var cancelConsumerTs = TimeSpan.FromSeconds(AwsTopicSharedFixture.DelaySeconds + cancelProducerTs.TotalSeconds);
        consumerCts.CancelAfter(cancelConsumerTs);

        testOutputHelper.WriteLine("Starting AWS Topic Test, to view ILogging output, please 'Debug' and view debug console.");

        var topicName = Guid.NewGuid().ToString();
        var subscribedQueues = new List<string>{Guid.NewGuid().ToString(), Guid.NewGuid().ToString()};

        var queueBuilder = sharedFixture.Builder
            .WithWorkerName("AwsTopicIntegrationTestWorker")
            .WithTopicName(topicName)
            .WithSubscribedQueues(subscribedQueues.ToArray())
            .WithQueueToSubscribeTo(subscribedQueues.First());

        var consumer = await queueBuilder.BuildConsumerAsync(consumerCts.Token);
        await consumer.ConsumeAsync(consumerCts.Token);

        try
        {
            var producer = await queueBuilder.BuildProducerAsync(producerCts.Token);

            while (!producerCts.IsCancellationRequested)
            {
                var messageGroupId = Guid.NewGuid().ToString();

                var message = new AwsTopicTestMessageV1
                {
                    ConversationId = Guid.NewGuid(),
                    EventId = MessageCounter.AwsTopicMessageSent.Count + 1,
                    EventName = "integration.test.v1",
                    Type = nameof(AwsTopicTestMessageV1),
                    MessageGroupId = messageGroupId,
                    MessageAdditionalProperties = new Dictionary<string, string>
                    {
                        { "traceparent", "traceparent" },
                        { "tracestate", "tracestate" }
                    }
                };

                var attributes = new Dictionary<string, string> { { "attr1", "value1" } };
                
                await producer.ProduceAsync(topicName, message, attributes, producerCts.Token);
                MessageCounter.AwsTopicMessageSent.Add(new MessageAttributePair{ BaseMessage = message , Attributes = attributes});

                await Task.Delay(TimeSpan.FromMinutes(5), consumerCts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            testOutputHelper.WriteLine($"Delay for: {AwsTopicSharedFixture.DelaySeconds} secs, to handle all messages.");
            await Task.Delay(TimeSpan.FromSeconds(AwsTopicSharedFixture.DelaySeconds), CancellationToken.None);
        }

        testOutputHelper.WriteLine($"Messages Sent: {MessageCounter.AwsTopicMessageSent.Count}");

        Assert.Equal(MessageCounter.AwsTopicMessageSent.Count, MessageCounter.AwsTopicMessageReceived.Count);
        foreach (var sent in MessageCounter.AwsTopicMessageReceived)
        {
            Assert.NotNull(sent.Attributes);
            Assert.Equal("AwsTopicIntegrationTestWorker", sent.Attributes["ProducerName"]);
            Assert.Equal("value1", sent.Attributes["attr1"]);
            Assert.Equal("traceparent", sent.Attributes["traceparent"]);
            Assert.Equal("tracestate", sent.Attributes["tracestate"]);
        }
    }
}
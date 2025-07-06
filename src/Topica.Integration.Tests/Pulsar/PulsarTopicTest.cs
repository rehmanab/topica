using Topica.Integration.Tests.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Topica.Integration.Tests.Pulsar;

[Trait("Category", "Integration"), Collection(nameof(PulsarTopicCollection))]
public class PulsarTopicTest(PulsarTopicSharedFixture sharedFixture, ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Produce_Consume_Message_Count_Equal()
    {
        MessageCounter.PulsarTopicMessageSent = [];
        MessageCounter.PulsarTopicMessageReceived = [];

        var producerCts = new CancellationTokenSource();
        var cancelProducerTs = TimeSpan.FromSeconds(5);
        producerCts.CancelAfter(cancelProducerTs);

        var consumerCts = new CancellationTokenSource();
        var cancelConsumerTs = TimeSpan.FromSeconds(PulsarTopicSharedFixture.DelaySeconds + cancelProducerTs.TotalSeconds);
        consumerCts.CancelAfter(cancelConsumerTs);

        testOutputHelper.WriteLine("Starting Pulsar Topic Test, to view ILogging output, please 'Debug' and view debug console.");

        var topicName = Guid.NewGuid().ToString();

        var queueBuilder = sharedFixture.Builder
            .WithWorkerName("PulsarTopicIntegrationTestWorker")
            .WithTopicName(topicName)
            .WithConsumerGroup("ConsumerGroup1")
            .WithConfiguration(tenant: "Test", @namespace: "Test", numberOfPartitions: 6)
            .WithTopicOptions(true);

        var consumer = await queueBuilder.BuildConsumerAsync(consumerCts.Token);
        await consumer.ConsumeAsync(consumerCts.Token);

        try
        {
            var producer = await queueBuilder.BuildProducerAsync(producerCts.Token);

            while (!producerCts.IsCancellationRequested)
            {
                var messageGroupId = Guid.NewGuid().ToString();

                var message = new PulsarTestMessageV1
                {
                    ConversationId = Guid.NewGuid(),
                    EventId = MessageCounter.PulsarTopicMessageSent.Count + 1,
                    EventName = "integration.test.v1",
                    Type = nameof(PulsarTestMessageV1),
                    MessageGroupId = messageGroupId,
                    MessageAdditionalProperties = new Dictionary<string, string>
                    {
                        { "traceparent", "traceparent" },
                        { "tracestate", "tracestate" }
                    }
                };
                
                var attributes = new Dictionary<string, string> { { "attr1", "value1" } };

                await producer.ProduceAsync(topicName, message, attributes, producerCts.Token);
                MessageCounter.PulsarTopicMessageSent.Add(new MessageAttributePair{ BaseMessage = message , Attributes = attributes});

                await Task.Delay(TimeSpan.FromMinutes(5), consumerCts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            testOutputHelper.WriteLine($"Delay for: {PulsarTopicSharedFixture.DelaySeconds} secs, to handle all messages.");
            await Task.Delay(TimeSpan.FromSeconds(PulsarTopicSharedFixture.DelaySeconds), CancellationToken.None);
        }

        testOutputHelper.WriteLine($"Messages Sent: {MessageCounter.PulsarTopicMessageSent.Count}");

        Assert.Equal(MessageCounter.PulsarTopicMessageSent.Count, MessageCounter.PulsarTopicMessageReceived.Count);
        foreach (var sent in MessageCounter.PulsarTopicMessageReceived)
        {
            Assert.NotNull(sent.Attributes);
            Assert.Equal("PulsarTopicIntegrationTestWorker", sent.Attributes["ProducerName"]);
            Assert.Equal("value1", sent.Attributes["attr1"]);
            Assert.Equal("traceparent", sent.Attributes["traceparent"]);
            Assert.Equal("tracestate", sent.Attributes["tracestate"]);
        }
    }
}
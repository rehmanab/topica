using Topica.Integration.Tests.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Topica.Integration.Tests.RabbitMq.RabbitMqQueue;

[Trait("Category", "Integration"), Collection(nameof(RabbitMqCollection))]
public class RabbitMqQueueTest(RabbitMqSharedFixture sharedFixture, ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Produce_Consume_Message_Count_Equal()
    {
        MessageCounter.RabbitMqQueueMessageSent = [];
        MessageCounter.RabbitMqQueueMessageReceived = [];

        var producerCts = new CancellationTokenSource();
        var cancelProducerTs = TimeSpan.FromSeconds(5);
        producerCts.CancelAfter(cancelProducerTs);

        var consumerCts = new CancellationTokenSource();
        var cancelConsumerTs = TimeSpan.FromSeconds(RabbitMqSharedFixture.DelaySeconds + cancelProducerTs.TotalSeconds);
        consumerCts.CancelAfter(cancelConsumerTs);

        testOutputHelper.WriteLine("Starting RabbitMq Queue Test, to view ILogging output, please 'Debug' and view debug console.");

        var queueName = Guid.NewGuid().ToString();

        var queueBuilder = sharedFixture.QueueBuilder
            .WithWorkerName("RabbitMqQueueIntegrationTestWorker")
            .WithQueueName(queueName);

        var consumer = await queueBuilder.BuildConsumerAsync(consumerCts.Token);
        await consumer.ConsumeAsync(consumerCts.Token);

        try
        {
            var producer = await queueBuilder.BuildProducerAsync(producerCts.Token);

            while (!producerCts.IsCancellationRequested)
            {
                var messageGroupId = Guid.NewGuid().ToString();

                var message = new RabbitMqQueueTestMessageV1
                {
                    ConversationId = Guid.NewGuid(),
                    EventId = MessageCounter.RabbitMqQueueMessageSent.Count + 1,
                    EventName = "integration.test.v1",
                    Type = nameof(RabbitMqQueueTestMessageV1),
                    MessageGroupId = messageGroupId,
                    MessageAdditionalProperties = new Dictionary<string, string>
                    {
                        { "traceparent", "traceparent" },
                        { "tracestate", "tracestate" }
                    }
                };
                
                var attributes = new Dictionary<string, string> { { "attr1", "value1" } };

                await producer.ProduceAsync(queueName, message, attributes, producerCts.Token);
                MessageCounter.RabbitMqQueueMessageSent.Add(new MessageAttributePair{ BaseMessage = message , Attributes = attributes});

                await Task.Delay(TimeSpan.FromMinutes(5), consumerCts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            testOutputHelper.WriteLine($"Delay for: {RabbitMqSharedFixture.DelaySeconds} secs, to handle all messages.");
            await Task.Delay(TimeSpan.FromSeconds(RabbitMqSharedFixture.DelaySeconds), CancellationToken.None);
        }

        testOutputHelper.WriteLine($"Messages Sent: {MessageCounter.RabbitMqQueueMessageSent.Count}");

        Assert.Equal(MessageCounter.RabbitMqQueueMessageSent.Count, MessageCounter.RabbitMqQueueMessageReceived.Count);
        foreach (var sent in MessageCounter.RabbitMqQueueMessageReceived)
        {
            Assert.NotNull(sent.Attributes);
            Assert.Equal("RabbitMqQueueIntegrationTestWorker", sent.Attributes["ProducerName"]);
            Assert.Equal("value1", sent.Attributes["attr1"]);
            Assert.Equal("traceparent", sent.Attributes["traceparent"]);
            Assert.Equal("tracestate", sent.Attributes["tracestate"]);
        }
    }   
}
using Topica.Integration.Tests.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Topica.Integration.Tests.Azure.ServiceBusQueue;

[Trait("Category", "Integration"), Collection(nameof(AzureServiceBusQueueCollection))]
public class AzureServiceBusQueueTest(AzureServiceBusQueueSharedFixture sharedFixture, ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Produce_Consume_Message_Count_Equal()
    {
        MessageCounter.AzureServiceBusQueueMessageSent = [];
        MessageCounter.AzureServiceBusQueueMessageReceived = [];

        var producerCts = new CancellationTokenSource();
        var cancelProducerTs = TimeSpan.FromSeconds(5);
        producerCts.CancelAfter(cancelProducerTs);

        var consumerCts = new CancellationTokenSource();
        var cancelConsumerTs = TimeSpan.FromSeconds(AzureServiceBusQueueSharedFixture.DelaySeconds + cancelProducerTs.TotalSeconds);
        consumerCts.CancelAfter(cancelConsumerTs);

        testOutputHelper.WriteLine("Starting Azure Service Bus Test, to view ILogging output, please 'Debug' and view debug console.");

        const string queueName = "integration_test_queue_1_v1";

        var queueBuilder = sharedFixture.Builder
            .WithWorkerName("AzureServiceBusQueueIntegrationTestWorker")
            .WithQueueName(queueName);

        var consumer = await queueBuilder.BuildConsumerAsync(consumerCts.Token);
        await consumer.ConsumeAsync(consumerCts.Token);

        try
        {
            var producer = await queueBuilder.BuildProducerAsync(producerCts.Token);
            Assert.Equal(queueName, producer.Source);

            while (!producerCts.IsCancellationRequested)
            {
                var messageGroupId = Guid.NewGuid().ToString();

                var message = new AzureServiceBusQueueTestMessageV1
                {
                    ConversationId = Guid.NewGuid(),
                    EventId = MessageCounter.AzureServiceBusQueueMessageSent.Count + 1,
                    EventName = "integration.test.v1",
                    Type = nameof(AzureServiceBusQueueTestMessageV1),
                    MessageGroupId = messageGroupId,
                    MessageAdditionalProperties = new Dictionary<string, string>
                    {
                        { "traceparent", "traceparent" },
                        { "tracestate", "tracestate" }
                    }
                };

                var attributes = new Dictionary<string, string> { { "attr1", "value1" } };
                
                await producer.ProduceAsync(message, attributes, producerCts.Token);
                MessageCounter.AzureServiceBusQueueMessageSent.Add(new MessageAttributePair{ BaseMessage = message , Attributes = attributes});

                await Task.Delay(TimeSpan.FromMinutes(5), consumerCts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            testOutputHelper.WriteLine($"Delay for: {AzureServiceBusQueueSharedFixture.DelaySeconds} secs, to handle all messages.");
            await Task.Delay(TimeSpan.FromSeconds(AzureServiceBusQueueSharedFixture.DelaySeconds), CancellationToken.None);
        }

        testOutputHelper.WriteLine($"Messages Sent: {MessageCounter.AzureServiceBusQueueMessageSent.Count}");

        Assert.Equal(MessageCounter.AzureServiceBusQueueMessageSent.Count, MessageCounter.AzureServiceBusQueueMessageReceived.Count);
        foreach (var sent in MessageCounter.AzureServiceBusQueueMessageReceived)
        {
            Assert.NotNull(sent.Attributes);
            Assert.Equal("AzureServiceBusQueueIntegrationTestWorker", sent.Attributes["ProducerName"]);
            Assert.Equal("value1", sent.Attributes["attr1"]);
            Assert.Equal("traceparent", sent.Attributes["traceparent"]);
            Assert.Equal("tracestate", sent.Attributes["tracestate"]);
        }
    }
}
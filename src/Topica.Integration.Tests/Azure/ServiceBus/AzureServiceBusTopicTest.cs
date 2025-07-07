using Topica.Integration.Tests.Shared;
using Topica.Settings;
using Xunit;
using Xunit.Abstractions;

namespace Topica.Integration.Tests.Azure.ServiceBus;

[Trait("Category", "Integration"), Collection(nameof(AzureServiceBusTopicCollection))]
public class AzureServiceBusTopicTest(AzureServiceBusTopicSharedFixture sharedFixture, ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Produce_Consume_Message_Count_Equal()
    {
        MessageCounter.AzureServiceBusTopicMessageSent = [];
        MessageCounter.AzureServiceBusTopicMessageReceived = [];

        var producerCts = new CancellationTokenSource();
        var cancelProducerTs = TimeSpan.FromSeconds(5);
        producerCts.CancelAfter(cancelProducerTs);

        var consumerCts = new CancellationTokenSource();
        var cancelConsumerTs = TimeSpan.FromSeconds(AzureServiceBusTopicSharedFixture.DelaySeconds + cancelProducerTs.TotalSeconds);
        consumerCts.CancelAfter(cancelConsumerTs);

        testOutputHelper.WriteLine("Starting Azure Service Bus Test, to view ILogging output, please 'Debug' and view debug console.");

        const string topicName = "integration_test_topic_1_v1";

        var subscriptions = new[]
        {
            new AzureServiceBusTopicSubscriptionSettings
            {
                Source = "integration_test_subscription_1_v1",
            },
            new AzureServiceBusTopicSubscriptionSettings
            {
                Source = "integration_test_subscription_2_v1",
            }
        };

        var queueBuilder = sharedFixture.Builder
            .WithWorkerName("AzureServiceBusIntegrationTestWorker")
            .WithTopicName(topicName)
            .WithSubscriptions(subscriptions)
            .WithSubscribeToSubscription("integration_test_subscription_1_v1");

        var consumer = await queueBuilder.BuildConsumerAsync(consumerCts.Token);
        await consumer.ConsumeAsync(consumerCts.Token);

        try
        {
            var producer = await queueBuilder.BuildProducerAsync(producerCts.Token);
            Assert.Equal(topicName, producer.Source);

            while (!producerCts.IsCancellationRequested)
            {
                var messageGroupId = Guid.NewGuid().ToString();

                var message = new AzureServiceBusTestMessageV1
                {
                    ConversationId = Guid.NewGuid(),
                    EventId = MessageCounter.AzureServiceBusTopicMessageSent.Count + 1,
                    EventName = "integration.test.v1",
                    Type = nameof(AzureServiceBusTestMessageV1),
                    MessageGroupId = messageGroupId,
                    MessageAdditionalProperties = new Dictionary<string, string>
                    {
                        { "traceparent", "traceparent" },
                        { "tracestate", "tracestate" }
                    }
                };

                var attributes = new Dictionary<string, string> { { "attr1", "value1" } };
                
                await producer.ProduceAsync(message, attributes, producerCts.Token);
                MessageCounter.AzureServiceBusTopicMessageSent.Add(new MessageAttributePair{ BaseMessage = message , Attributes = attributes});

                await Task.Delay(TimeSpan.FromMinutes(5), consumerCts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            testOutputHelper.WriteLine($"Delay for: {AzureServiceBusTopicSharedFixture.DelaySeconds} secs, to handle all messages.");
            await Task.Delay(TimeSpan.FromSeconds(AzureServiceBusTopicSharedFixture.DelaySeconds), CancellationToken.None);
        }

        testOutputHelper.WriteLine($"Messages Sent: {MessageCounter.AzureServiceBusTopicMessageSent.Count}");

        Assert.Equal(MessageCounter.AzureServiceBusTopicMessageSent.Count, MessageCounter.AzureServiceBusTopicMessageReceived.Count);
        foreach (var sent in MessageCounter.AzureServiceBusTopicMessageReceived)
        {
            Assert.NotNull(sent.Attributes);
            Assert.Equal("AzureServiceBusIntegrationTestWorker", sent.Attributes["ProducerName"]);
            Assert.Equal("value1", sent.Attributes["attr1"]);
            Assert.Equal("traceparent", sent.Attributes["traceparent"]);
            Assert.Equal("tracestate", sent.Attributes["tracestate"]);
        }
    }
}
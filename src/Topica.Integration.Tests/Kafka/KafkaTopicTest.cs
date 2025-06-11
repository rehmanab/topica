using Topica.Integration.Tests.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Topica.Integration.Tests.Kafka;

[Trait("Category", "Integration"), Collection(nameof(KafkaTopicCollection))]
public class KafkaTopicTest(KafkaTopicSharedFixture sharedFixture, ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Produce_Consume_Message_Count_Equal()
    {
        MessageCounter.KafkaTopicMessageSent = [];
        MessageCounter.KafkaTopicMessageReceived = [];

        var producerCts = new CancellationTokenSource();
        var cancelProducerTs = TimeSpan.FromSeconds(5);
        producerCts.CancelAfter(cancelProducerTs);

        var consumerCts = new CancellationTokenSource();
        var cancelConsumerTs = TimeSpan.FromSeconds(KafkaTopicSharedFixture.DelaySeconds + cancelProducerTs.TotalSeconds);
        consumerCts.CancelAfter(cancelConsumerTs);

        testOutputHelper.WriteLine("Starting Kafka Topic Test, to view ILogging output, please 'Debug' and view debug console.");

        var topicName = Guid.NewGuid().ToString();

        var queueBuilder = sharedFixture.Builder
            .WithWorkerName("KafkaTopicIntegrationTestWorker")
            .WithTopicName(topicName)
            .WithConsumerGroup("ConsumerGroup1")
            .WithTopicSettings(true, 6)
            .WithBootstrapServers([sharedFixture.BootstrapServerAddress]);

        var consumer = await queueBuilder.BuildConsumerAsync(consumerCts.Token);
        await consumer.ConsumeAsync(consumerCts.Token);

        try
        {
            var producer = await queueBuilder.BuildProducerAsync(producerCts.Token);

            while (!producerCts.IsCancellationRequested)
            {
                var messageGroupId = Guid.NewGuid().ToString();

                var message = new KafkaTestMessageV1
                {
                    ConversationId = Guid.NewGuid(),
                    EventId = MessageCounter.KafkaTopicMessageSent.Count + 1,
                    EventName = "integration.test.v1",
                    Type = nameof(KafkaTestMessageV1),
                    MessageGroupId = messageGroupId
                };

                await producer.ProduceAsync(topicName, message, null, producerCts.Token);
                MessageCounter.KafkaTopicMessageSent.Add(message);

                await Task.Delay(TimeSpan.FromSeconds(1), producerCts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            testOutputHelper.WriteLine($"Delay for: {KafkaTopicSharedFixture.DelaySeconds} secs, to handle all messages.");
            await Task.Delay(TimeSpan.FromSeconds(KafkaTopicSharedFixture.DelaySeconds), CancellationToken.None);
        }

        testOutputHelper.WriteLine($"Messages Sent: {MessageCounter.KafkaTopicMessageSent.Count}");

        Assert.Equal(MessageCounter.KafkaTopicMessageSent.Count, MessageCounter.KafkaTopicMessageReceived.Count);
        Assert.Equivalent(MessageCounter.KafkaTopicMessageSent, MessageCounter.KafkaTopicMessageReceived);
    }   
}
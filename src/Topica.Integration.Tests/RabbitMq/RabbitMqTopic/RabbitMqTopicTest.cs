using Topica.Integration.Tests.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Topica.Integration.Tests.RabbitMq.RabbitMqTopic;

[Trait("Category", "Integration"), Collection(nameof(RabbitMqCollection))]
public class RabbitMqTopicTest(RabbitMqSharedFixture sharedFixture, ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Produce_Consume_Message_Count_Equal()
    {
        MessageCounter.RabbitMqTopicMessageSent = [];
        MessageCounter.RabbitMqTopicMessageReceived = [];

        var producerCts = new CancellationTokenSource();
        var cancelProducerTs = TimeSpan.FromSeconds(5);
        producerCts.CancelAfter(cancelProducerTs);

        var consumerCts = new CancellationTokenSource();
        var cancelConsumerTs = TimeSpan.FromSeconds(RabbitMqSharedFixture.DelaySeconds + cancelProducerTs.TotalSeconds);
        consumerCts.CancelAfter(cancelConsumerTs);

        testOutputHelper.WriteLine("Starting RabbitMq Topic Test, to view ILogging output, please 'Debug' and view debug console.");

        var topicName = Guid.NewGuid().ToString();
        var subscribedQueues = new List<string>{Guid.NewGuid().ToString(), Guid.NewGuid().ToString()};

        var topicBuilder = sharedFixture.TopicBuilder
            .WithWorkerName("RabbitMqTopicIntegrationTestWorker")
            .WithTopicName(topicName)
            .WithSubscribedQueues(subscribedQueues.ToArray())
            .WithQueueToSubscribeTo(subscribedQueues.First());

        var consumer = await topicBuilder.BuildConsumerAsync(consumerCts.Token);
        await consumer.ConsumeAsync(consumerCts.Token);

        try
        {
            var producer = await topicBuilder.BuildProducerAsync(producerCts.Token);

            while (!producerCts.IsCancellationRequested)
            {
                var messageGroupId = Guid.NewGuid().ToString();

                var message = new RabbitMqTopicTestMessageV1
                {
                    ConversationId = Guid.NewGuid(),
                    EventId = MessageCounter.RabbitMqTopicMessageSent.Count + 1,
                    EventName = "integration.test.v1",
                    Type = nameof(RabbitMqTopicTestMessageV1),
                    MessageGroupId = messageGroupId
                };

                await producer.ProduceAsync(topicName, message, null, producerCts.Token);
                MessageCounter.RabbitMqTopicMessageSent.Add(message);

                await Task.Delay(TimeSpan.FromMinutes(5), consumerCts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            testOutputHelper.WriteLine($"Delay for: {RabbitMqSharedFixture.DelaySeconds} secs, to handle all messages.");
            await Task.Delay(TimeSpan.FromSeconds(RabbitMqSharedFixture.DelaySeconds), CancellationToken.None);
        }

        testOutputHelper.WriteLine($"Messages Sent: {MessageCounter.RabbitMqTopicMessageSent.Count}");

        Assert.Equal(MessageCounter.RabbitMqTopicMessageSent.Count, MessageCounter.RabbitMqTopicMessageReceived.Count);
        Assert.Equivalent(MessageCounter.RabbitMqTopicMessageSent, MessageCounter.RabbitMqTopicMessageReceived);
    }   
}
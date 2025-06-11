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
                    MessageGroupId = messageGroupId
                };

                await producer.ProduceAsync(queueName, message, null, producerCts.Token);
                MessageCounter.RabbitMqQueueMessageSent.Add(message);

                await Task.Delay(TimeSpan.FromSeconds(1), producerCts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            testOutputHelper.WriteLine($"Delay for: {RabbitMqSharedFixture.DelaySeconds} secs, to handle all messages.");
            await Task.Delay(TimeSpan.FromSeconds(RabbitMqSharedFixture.DelaySeconds), CancellationToken.None);
        }

        testOutputHelper.WriteLine($"Messages Sent: {MessageCounter.RabbitMqQueueMessageSent.Count}");

        Assert.Equal(MessageCounter.RabbitMqQueueMessageSent.Count, MessageCounter.RabbitMqQueueMessageReceived.Count);
        Assert.Equivalent(MessageCounter.RabbitMqQueueMessageSent, MessageCounter.RabbitMqQueueMessageReceived);
    }   
}
using Topica.Integration.Tests.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Topica.Integration.Tests.Aws.AwsQueue;

[Trait("Category", "Integration"), Collection(nameof(AwsQueueCollection))]
public class AwsQueueTest(AwsQueueSharedFixture sharedFixture, ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Produce_Consume_Message_Count_Equal()
    {
        MessageCounter.AwsQueueMessageSent = [];
        MessageCounter.AwsQueueMessageReceived = [];

        var producerCts = new CancellationTokenSource();
        var cancelProducerTs = TimeSpan.FromSeconds(5);
        producerCts.CancelAfter(cancelProducerTs);

        var consumerCts = new CancellationTokenSource();
        var cancelConsumerTs = TimeSpan.FromSeconds(AwsQueueSharedFixture.DelaySeconds + cancelProducerTs.TotalSeconds);
        consumerCts.CancelAfter(cancelConsumerTs);

        testOutputHelper.WriteLine("Starting AWS Queue Test, to view ILogging output, please 'Debug' and view debug console.");

        var queueName = Guid.NewGuid().ToString();

        var queueBuilder = sharedFixture.Builder
            .WithWorkerName("AwsQueueIntegrationTestWorker")
            .WithQueueName(queueName);

        var consumer = await queueBuilder.BuildConsumerAsync(consumerCts.Token);
        await consumer.ConsumeAsync(consumerCts.Token);

        try
        {
            var producer = await queueBuilder.BuildProducerAsync(producerCts.Token);

            while (!producerCts.IsCancellationRequested)
            {
                var messageGroupId = Guid.NewGuid().ToString();

                var message = new AwsQueueTestMessageV1
                {
                    ConversationId = Guid.NewGuid(),
                    EventId = MessageCounter.AwsQueueMessageSent.Count + 1,
                    EventName = "integration.test.v1",
                    Type = nameof(AwsQueueTestMessageV1),
                    MessageGroupId = messageGroupId
                };

                var messageAttributes = new Dictionary<string, string> { { "SignatureVersion", "2" } };

                await producer.ProduceAsync(queueName, message, messageAttributes, producerCts.Token);
                MessageCounter.AwsQueueMessageSent.Add(message);

                await Task.Delay(TimeSpan.FromMinutes(5), consumerCts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            testOutputHelper.WriteLine($"Delay for: {AwsQueueSharedFixture.DelaySeconds} secs, to handle all messages.");
            await Task.Delay(TimeSpan.FromSeconds(AwsQueueSharedFixture.DelaySeconds), CancellationToken.None);
        }

        testOutputHelper.WriteLine($"Messages Sent: {MessageCounter.AwsQueueMessageSent.Count}");

        Assert.Equal(MessageCounter.AwsQueueMessageSent.Count, MessageCounter.AwsQueueMessageReceived.Count);
        Assert.Equivalent(MessageCounter.AwsQueueMessageSent, MessageCounter.AwsQueueMessageReceived);
    }
}
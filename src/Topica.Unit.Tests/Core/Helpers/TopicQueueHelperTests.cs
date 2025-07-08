using Topica.Aws.Helpers;

namespace Topica.Unit.Tests.Core.Helpers;

[Trait("Category", "Unit")]
public class TopicQueueHelperTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("queue.name.1")]
    [InlineData("queue.name.1.fifo")]
    [InlineData("xxx-yyy-zzz.whatever")]
    public void AddTopicQueueNameFifoSuffix_Correctly_Return_Name_Same_As_Source_When_Not_Fifo(string? source)
    {
        Assert.Equal(source, TopicQueueHelper.AddTopicQueueNameFifoSuffix(source!, false));
    }

    [Theory]
    [InlineData("queue.name.1")]
    [InlineData("queue.name.1.fifo")]
    [InlineData("xxx-yyy-zzz.whatever")]
    [InlineData("queue.name.1.fifo.fifo.fifo.fifo")]
    public void AddTopicQueueNameFifoSuffix_Correctly_Return_Name_With_Fifo_Suffix(string? source)
    {
        Assert.EndsWith(Constants.FifoSuffix, TopicQueueHelper.AddTopicQueueNameFifoSuffix(source!, true));
    }
    
    [Theory]
    [InlineData("queue.name.1.fifo", true)]
    [InlineData("queue.name.1.fifo", false)]
    [InlineData("queue.name.1.fifo.fifo.fifo.fifo", true)]
    [InlineData("queue.name.1.fifo.fifo.fifo.fifo", false)]
    public void AddTopicQueueNameFifoSuffix_Does_Not_Add_Fifo_Suffix_When_Source_Ends_With_Fifo_Suffix(string? source, bool isFifo)
    {
        Assert.Equal(source, TopicQueueHelper.AddTopicQueueNameFifoSuffix(source!, isFifo));
    }
}
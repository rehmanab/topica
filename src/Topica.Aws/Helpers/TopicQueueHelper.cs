namespace Topica.Aws.Helpers;

public static class TopicQueueHelper
{
    public static string AddTopicQueueNameFifoSuffix(string name, bool isFifo)
    {
        return !string.IsNullOrWhiteSpace(name) && isFifo && !name.EndsWith(Constants.FifoSuffix) ? $"{name}{Constants.FifoSuffix}" : name;
    }
    
    public static string RemoveTopicQueueNameFifoSuffix(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && name.EndsWith(Constants.FifoSuffix) ? name[..^Constants.FifoSuffix.Length] : name;
    }
}
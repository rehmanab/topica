namespace Topica.Aws.Helpers;

public static class TopicQueueHelper
{
    public static string AddTopicQueueNameFifoSuffix(string name, bool isFifo)
    {
        return !string.IsNullOrWhiteSpace(name) && isFifo && !name.EndsWith(Constants.FifoSuffix) ? $"{name}{Constants.FifoSuffix}" : name;
    }
    
    public static string AddTopicQueueNameErrorAndFifoSuffix(string name, bool isFifo)
    {
        return AddTopicQueueNameFifoSuffix($"{RemoveTopicQueueNameFifoSuffix(name)}{Constants.ErrorQueueSuffix}", isFifo);
    }
    
    public static string RemoveTopicQueueNameFifoSuffix(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && name.EndsWith(Constants.FifoSuffix) ? name[..^Constants.FifoSuffix.Length] : name;
    }
}
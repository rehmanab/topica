using System;

namespace Topica.Aws.Queues
{
    public class AwsQueueAttributesException : Exception
    {
        public AwsQueueAttributesException(string message) : base(message)
        {
            
        }
    }
}
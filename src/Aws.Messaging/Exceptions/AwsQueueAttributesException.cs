using System;

namespace Aws.Messaging.Exceptions
{
    public class AwsQueueAttributesException : Exception
    {
        public AwsQueueAttributesException(string message) : base(message)
        {
            
        }
    }
}
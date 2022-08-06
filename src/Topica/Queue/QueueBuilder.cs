using System;
using System.Collections.Generic;
using System.Linq;
using Topica.Contracts;

namespace Topica.Queue
{
    public interface IQueueBuilder
    {
        IQueueOptionalSettings WithQueueName(string queueName);
        IQueueOptionalSettings WithQueueNames(IEnumerable<string> queueNames);
        IQueueOptionalSettings WithQueueNamesFromUrl(IEnumerable<string> queueUrls);
    }
    
    public class QueueBuilder : IQueueBuilder
    {
        private readonly IQueueProvider _queueProvider;

        public QueueBuilder(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }
        
        public IQueueOptionalSettings WithQueueName(string queueName)
        {
            return new QueueOptionalSettings(_queueProvider, queueName); 
        }

        public IQueueOptionalSettings WithQueueNames(IEnumerable<string> queueNames)
        {
            return new QueueOptionalSettings(_queueProvider, queueNames); 
        }

        public IQueueOptionalSettings WithQueueNamesFromUrl(IEnumerable<string> queueUrls)
        {
            var queueNames = queueUrls.Select(x =>
            {
                if (x.Contains("http://") || x.Contains("https://"))
                {
                    return x.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                }

                return x;
            });

            return WithQueueNames(queueNames);
        }
    }
}
using System.Threading.Tasks;
using Aws.Messaging.Config;
using Aws.Messaging.Contracts;

namespace Aws.Messaging.Queue
{
    public interface IQueueOptionalSettings
    {
        IQueueOptionalSettings WithQueueConfiguration(SqsConfiguration sqsConfiguration);
        Task<string> BuildAsync();
    }
    
    public class QueueOptionalSettings : IQueueOptionalSettings
    {
        private readonly string _name;
        private readonly IQueueProvider _queueProvider;
        private SqsConfiguration _sqsConfiguration;

        public QueueOptionalSettings(string name, IQueueProvider queueProvider)
        {
            _name = name;
            _queueProvider = queueProvider;
        }
        
        public IQueueOptionalSettings WithQueueConfiguration(SqsConfiguration sqsConfiguration)
        {
            _sqsConfiguration = sqsConfiguration;
            return this;
        }

        public async Task<string> BuildAsync()
        {
            return await _queueProvider.CreateQueueAsync(_name, _sqsConfiguration ?? new SqsConfiguration());
        }
    }
}
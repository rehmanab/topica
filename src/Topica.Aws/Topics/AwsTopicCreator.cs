using System;
using System.Threading.Tasks;
using Topica.Aws.Configuration;
using Topica.Contracts;
using Topica.Topics;

namespace Topica.Aws.Topics
{
    public class AwsTopicCreator : ITopicCreator
    {
        private readonly IAwsTopicBuilder _awsTopicBuilder;
        private readonly ISqsConfigurationBuilder _sqsConfigurationBuilder;
        public MessagingPlatform MessagingPlatform => MessagingPlatform.Aws;

        public AwsTopicCreator(IAwsTopicBuilder awsTopicBuilder, ISqsConfigurationBuilder sqsConfigurationBuilder)
        {
            _awsTopicBuilder = awsTopicBuilder;
            _sqsConfigurationBuilder = sqsConfigurationBuilder;
        }

        public async Task<string> CreateTopic(TopicConfigurationBase configuration)
        {
            var config = configuration as AwsTopicConfiguration;

            if (config == null)
            {
                throw new Exception($"{nameof(AwsTopicCreator)}.{nameof(CreateTopic)} needs an {nameof(AwsTopicConfiguration)} ");
            }
            
            var creator = _awsTopicBuilder.WithTopicName(configuration.TopicName);
            foreach (var subscribedQueueName in config.WithSubscribedQueues)
            {
                creator = creator.WithSubscribedQueue(subscribedQueueName);
            }

            if (config.BuildWithErrorQueue)
            {
                creator = creator.WithQueueConfiguration(_sqsConfigurationBuilder.BuildCreateWithErrorQueue(config.ErrorQueueMaxReceiveCount ?? 5));
            }
            
            return await creator.BuildAsync();
        }
    }
}
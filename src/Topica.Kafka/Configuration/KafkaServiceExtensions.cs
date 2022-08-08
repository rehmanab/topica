using Microsoft.Extensions.Logging;
using Topica;
using Topica.Contracts;
using Topica.Kafka.Settings;
using Topica.Kafka.Topics;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class KafkaServiceExtensions
    {
        public static IServiceCollection AddKafkaTopica(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            
            var logger = serviceProvider.GetService<ILogger<MessagingPlatform>>();
            logger.LogDebug("******* Kafka Service Extensions ******* ");
            
            var kafkaSettings = serviceProvider.GetService<KafkaSettings>();

            services.AddScoped<IConsumer, KafkaTopicConsumer>();

            return services;
        }
    }
}
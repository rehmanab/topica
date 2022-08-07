using Microsoft.Extensions.Logging;
using Topica;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KafkaServiceExtensions
    {
        public static IServiceCollection AddKafkaTopica(this IServiceCollection services, string serviceUrl)
        {
            var logger = services.BuildServiceProvider().GetService<ILogger<MessagingPlatform>>();
            logger.LogDebug("******* KafkaServiceExtensions ******* ");

            return services;
        }
    }
}
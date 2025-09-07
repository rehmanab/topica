using HealthChecks.UI.Core;
using Topica.Web.Configuration.WebServices;
using Topica.Web.HealthChecks;
using Topica.Web.Models;
using static Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class WebHealthCheckExtensions
{
    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services, Action<WebConfiguration> configuration)
    {
        var config = new WebConfiguration();
        configuration(config);
        
        var tags = new List<string>();

        // Health Checks and Health Checks UI (https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks)
        var healthChecksBuilder = services.AddHealthChecks();
        
        tags.Add(nameof(HealthCheckTag.Local));
        healthChecksBuilder.AddCheck("Topica Platforms Running", () => Healthy(), tags: [nameof(HealthCheckTag.Local)]);
        
        tags.Add(nameof(HealthCheckTag.Ping));
        healthChecksBuilder.AddUrlGroup(new Uri("https://mock.httpstatus.io/200"), name: "https://mock.httpstatus.io/200", tags: [nameof(HealthCheckTag.Ping)]);
        
        if (config.HealthCheckSettings.AwsQueue.Enabled)
        {
            tags.Add(config.HealthCheckSettings.AwsQueue.Tag.ToString());
            healthChecksBuilder.AddCheck<AwsQueueHealthCheck>(config.HealthCheckSettings.AwsQueue.Name, timeout: config.HealthCheckSettings.AwsQueue.TimeOut, tags: [config.HealthCheckSettings.AwsQueue.Tag.ToString()]);
        }

        if (config.HealthCheckSettings.AwsTopic.Enabled)
        {
            tags.Add(config.HealthCheckSettings.AwsTopic.Tag.ToString());
            healthChecksBuilder.AddCheck<AwsTopicHealthCheck>(config.HealthCheckSettings.AwsTopic.Name, timeout: config.HealthCheckSettings.AwsTopic.TimeOut, tags: [config.HealthCheckSettings.AwsTopic.Tag.ToString()]);
        }

        if (config.HealthCheckSettings.AzureServiceBus.Enabled)
        {
            tags.Add(config.HealthCheckSettings.AzureServiceBus.Tag.ToString());
            healthChecksBuilder.AddCheck<AzureServiceBusHealthCheck>(config.HealthCheckSettings.AzureServiceBus.Name, timeout: config.HealthCheckSettings.AzureServiceBus.TimeOut, tags: [config.HealthCheckSettings.AzureServiceBus.Tag.ToString()]);
        }

        if (config.HealthCheckSettings.Kafka.Enabled)
        {
            tags.Add(config.HealthCheckSettings.Kafka.Tag.ToString());
            healthChecksBuilder.AddCheck<KafkaHealthCheck>(config.HealthCheckSettings.Kafka.Name, timeout: config.HealthCheckSettings.Kafka.TimeOut, tags: [config.HealthCheckSettings.Kafka.Tag.ToString()]);
        }

        if (config.HealthCheckSettings.Pulsar.Enabled)
        {
            tags.Add(config.HealthCheckSettings.Pulsar.Tag.ToString());
            healthChecksBuilder.AddCheck<PulsarHealthCheck>(config.HealthCheckSettings.Pulsar.Name, timeout: config.HealthCheckSettings.Pulsar.TimeOut, tags: [config.HealthCheckSettings.Pulsar.Tag.ToString()]);
        }

        if (config.HealthCheckSettings.RabbitMqQueue.Enabled)
        {
            tags.Add(config.HealthCheckSettings.RabbitMqQueue.Tag.ToString());
            healthChecksBuilder.AddCheck<RabbitMqQueueHealthCheck>(config.HealthCheckSettings.RabbitMqQueue.Name, timeout: config.HealthCheckSettings.RabbitMqQueue.TimeOut, tags: [config.HealthCheckSettings.RabbitMqQueue.Tag.ToString()]);
        }

        if (config.HealthCheckSettings.RabbitMqTopic.Enabled)
        {
            tags.Add(config.HealthCheckSettings.RabbitMqTopic.Tag.ToString());
            healthChecksBuilder.AddCheck<RabbitMqTopicHealthCheck>(config.HealthCheckSettings.RabbitMqTopic.Name, timeout: config.HealthCheckSettings.RabbitMqTopic.TimeOut, tags: [config.HealthCheckSettings.RabbitMqTopic.Tag.ToString()]);
        }

        services.AddHealthChecksUI(setup =>
        {
            setup.SetHeaderText("Health Checks Status - Topica platforms");
            setup.SetEvaluationTimeInSeconds(30);
            setup.SetMinimumSecondsBetweenFailureNotifications(60);
            setup.MaximumHistoryEntriesPerEndpoint(100);
            setup.SetApiMaxActiveRequests(1);

            tags.Distinct().ToList().ForEach(x => setup.AddHealthCheckEndpoint(x, $"http://localhost:7022/api/health/{x}"));

            setup.AddWebhookNotification("Webhook (https://memquran.requestcatcher.com)", uri: "https://memquran.requestcatcher.com/anything",
                payload: "{ \"message\": \"Webhook report for [[LIVENESS]] Health Check: [[FAILURE]] - Description: [[DESCRIPTIONS]]\"}",
                restorePayload: "{ \"message\": \"[[LIVENESS]] Health Check is back to life\"}",
                shouldNotifyFunc: (livenessName, report) => DateTime.UtcNow.Hour >= 8 && DateTime.UtcNow.Hour <= 23,
                customMessageFunc: (livenessName, report) =>
                {
                    var failing = report.Entries.Where(e => e.Value.Status == UIHealthStatus.Unhealthy);
                    return $"{failing.Count()} healthchecks are failing";
                }, customDescriptionFunc: (livenessName, report) =>
                {
                    var failing = report.Entries.Where(e => e.Value.Status == UIHealthStatus.Unhealthy);
                    return $"{string.Join(" - ", failing.Select(f => f.Key))} healthchecks are failing";
                });
        }).AddInMemoryStorage();

        return services;
    }
}
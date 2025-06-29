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
        
        // Health Checks and Health Checks UI (https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks)
        services.AddHealthChecks()
            .AddCheck("Topica Platforms Running", () => Healthy(), tags: [nameof(HealthCheckTags.Local)])
            .AddCheck<AwsQueueHealthCheck>("Aws Queue", timeout: TimeSpan.FromSeconds(15), tags: [nameof(HealthCheckTags.Aws)])
            .AddCheck<AwsTopicHealthCheck>("Aws Topic", timeout: TimeSpan.FromSeconds(15), tags: [nameof(HealthCheckTags.Aws)])
            .AddCheck<AzureServiceBusHealthCheck>("Azure ServiceBus Topic", timeout: TimeSpan.FromSeconds(15), tags: [nameof(HealthCheckTags.Azure)])
            .AddCheck<KafkaHealthCheck>("Kafka Topic", timeout: TimeSpan.FromMinutes(2), tags: [nameof(HealthCheckTags.Kafka)])
            .AddCheck<PulsarHealthCheck>("Pulsar Topic", timeout: TimeSpan.FromMinutes(2), tags: [nameof(HealthCheckTags.Pulsar)])
            .AddCheck<RabbitMqQueueHealthCheck>("RabbitMq Queue", timeout: TimeSpan.FromSeconds(15), tags: [nameof(HealthCheckTags.RabbitMq)])
            .AddCheck<RabbitMqTopicHealthCheck>("RabbitMq Topic", timeout: TimeSpan.FromSeconds(15), tags: [nameof(HealthCheckTags.RabbitMq)])
            .AddUrlGroup(new Uri("https://mock.httpstatus.io/200"), name: "https://mock.httpstatus.io/200", tags: [nameof(HealthCheckTags.Ping)]);
        
        services.AddHealthChecksUI(setup =>
        {
            setup.SetHeaderText("Health Checks Status - Topica platforms");
            setup.SetEvaluationTimeInSeconds(30);
            setup.SetMinimumSecondsBetweenFailureNotifications(60);
            setup.MaximumHistoryEntriesPerEndpoint(100);
            setup.SetApiMaxActiveRequests(1);
            
            var tags = Enum.GetValues<HealthCheckTags>().Select(x => x.ToString()).ToList();
            tags.ForEach(x => setup.AddHealthCheckEndpoint(x, $"http://localhost:7022/api/health/{x}"));
            
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
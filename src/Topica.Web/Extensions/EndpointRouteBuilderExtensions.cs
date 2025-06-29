using System.Text.Json;
using System.Text.Json.Serialization;
using HealthChecks.UI.Client;
using HealthChecks.UI.Core;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Topica.Web.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapCustomHealthCheck(
        this IEndpointRouteBuilder endpoints,
        IEnumerable<string> tags)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        foreach (var tag in tags)
        {
            endpoints.MapHealthChecks($"/api/health/{tag}", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(tag),
                AllowCachingResponses = false,
                ResponseWriter = WriteResponse,
                ResultStatusCodes = GetResultStatusCodes()
            });
        }

        return endpoints;
    }

    private static async Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        
        var uiReport = UIHealthReport.CreateFrom(report);
        
        await context.Response.WriteAsJsonAsync(uiReport, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        });
    }

    private static Dictionary<HealthStatus, int> GetResultStatusCodes()
    {
        return new Dictionary<HealthStatus, int>
        {
            { HealthStatus.Healthy, StatusCodes.Status200OK },
            { HealthStatus.Degraded, StatusCodes.Status503ServiceUnavailable },
            { HealthStatus.Unhealthy, StatusCodes.Status503ServiceUnavailable }
        };
    }
}
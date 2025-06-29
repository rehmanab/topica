namespace Topica.Web.Configuration.WebServices;

public class WebConfiguration
{
    public int HealthCheckTimeoutSeconds { get; set; }

    public IWebHostEnvironment Environment { get; set; } = null!;
}
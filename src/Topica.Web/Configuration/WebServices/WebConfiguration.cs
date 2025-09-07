using Topica.Web.Settings;

namespace Topica.Web.Configuration.WebServices;

public class WebConfiguration
{
    public HealthCheckSettings HealthCheckSettings { get; set; } = null!;

    public IWebHostEnvironment Environment { get; set; } = null!;
}
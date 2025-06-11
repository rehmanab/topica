namespace Topica.Integration.Tests.Shared;

public static class Utilities
{
    public static string GetConnectionString(string hostname, int publicPort)
    {
        var properties = new Dictionary<string, string>
        {
            { "Endpoint", new UriBuilder("sb", hostname, publicPort).ToString() },
            { "SharedAccessKeyName", "RootManageSharedAccessKey" },
            { "SharedAccessKey", "SAS_KEY_VALUE" },
            { "UseDevelopmentEmulator", "true" }
        };
        return string.Join(";", properties.Select(property => string.Join("=", property.Key, property.Value)));
    }
}
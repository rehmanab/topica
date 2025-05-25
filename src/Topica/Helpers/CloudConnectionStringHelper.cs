using System.Collections.Generic;
using System.Linq;

namespace Topica.Helpers;

public static class CloudConnectionStringHelper
{
    public static Dictionary<string, string> ParseCloudConnectionString(string connectionString)
    {
        return connectionString.Split(';')
            .Where(x => x.Length > 0).Select(x => x.Split(['='], 2))
            .ToDictionary(x => x[0], x => x[1]);
    }
    
    public static string ParseEndpointCloudConnectionString(string connectionString)
    {
        var dic = ParseCloudConnectionString(connectionString);
        
        return dic.ContainsKey("Endpoint") ? dic["Endpoint"].TrimEnd('/') : null;
    }
}
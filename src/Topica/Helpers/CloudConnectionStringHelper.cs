using System;
using System.Collections.Generic;
using System.Linq;

namespace Topica.Helpers;

public static class CloudConnectionStringHelper
{
    public static string? ParseEndpointCloudConnectionString(string? connectionString)
    {
        var dic = ParseCloudConnectionString(connectionString);
        
        return dic.ContainsKey("endpoint") ? dic["endpoint"].TrimEnd('/') : null;
    }

    private static Dictionary<string, string> ParseCloudConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) return new Dictionary<string, string>();

        var stringsEnumerable = connectionString
            .ToLower()
            .Split(';')
            .Where(x => !string.IsNullOrWhiteSpace(x) && x.Length > 1)
            .Select(x => x.Split(['='], 2))
            .Where(x => x.Length >= 2)
            .Where(x => !string.IsNullOrWhiteSpace(x[0]) && !string.IsNullOrWhiteSpace(x[1]));

        var enumerable = stringsEnumerable.ToList();
        
        if (!enumerable.Any()) return new Dictionary<string, string>();
        
        var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var item in enumerable)
        {
            if (item.Length != 2) continue;
            var key = item[0].Trim();
            var value = item[1].Trim();
            dictionary.TryAdd(key, value);
        }
        
        return dictionary;
    }
}
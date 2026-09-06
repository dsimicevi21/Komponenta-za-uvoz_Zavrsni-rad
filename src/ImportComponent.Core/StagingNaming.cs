using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportComponent.Core;

public static class StagingNaming
{
    private const string Prefix = "staging_";

    public static string NewRunId() => $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{Guid.NewGuid():N}";

    public static bool TryParseCreatedAt(string stagingName, out DateTimeOffset createdAt)
    {
        createdAt = default;

        if (!stagingName.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return false;
        }

        var timestampPart = stagingName[Prefix.Length..].Split('_', 2)[0];
    
        if (!long.TryParse(timestampPart, out var unixSeconds))
        {
            return false;
        }
        createdAt = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        return true;
        
    }
}
 

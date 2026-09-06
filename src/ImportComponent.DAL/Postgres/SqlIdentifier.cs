using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ImportComponent.DAL.Postgres;

internal static class SqlIdentifier
{
    private static readonly Regex ValidPattern = new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);
    public static string Quote(string identifier)
    {
        if (!ValidPattern.IsMatch(identifier))
        {
            throw new ArgumentException($" '{identifier}' is not a valid SQL identifier.", nameof(identifier));
        }
        return $"\"{identifier}\"";
    }
    public static string SanitizeRunId(string runId)
    {
        var cleaned = new string(runId.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        return $"staging_{cleaned}";
    }
}

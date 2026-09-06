using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using ImportComponent.Core;

namespace ImportComponent.BLL;

public static class FieldMappingSuggester
{
    private const double MinimumConfidence = 0.6;

    public static IReadOnlyDictionary<string, string> Suggest(
        IReadOnlyList<string> sourceFieldNames,
        IReadOnlyList<ColumnInfo> targetColumns)
    {
        var suggestions = new Dictionary<string, string>();
        var usedTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var sourceFIeld in sourceFieldNames)
        {
            var bestTarget = FindBestMatch(sourceFIeld, targetColumns, usedTargets);
            if (bestTarget is not null)
            {
                suggestions[sourceFIeld] = bestTarget;
                usedTargets.Add(bestTarget);
            }
        }
        return suggestions;
    }

    private static string? FindBestMatch(string sourceField, IReadOnlyList<ColumnInfo> targetColumns, HashSet<string> usedTargets)
    {
        var normalizedSource = Normalize(sourceField);

        string? bestTarget = null;
        var bestScore = 0.0;

        foreach (var column in targetColumns)
        {
            if (usedTargets.Contains(column.Name))
            {
                continue;
            }

            var score = SimilarityScore(normalizedSource, Normalize(column.Name));
            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = column.Name;
            }
        }
        return bestScore >= MinimumConfidence ? bestTarget : null;
    }
    private static string Normalize(string name) =>
        new string(name.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

    private static double SimilarityScore(string a, string b)
    {
        if (a == b)
        {
            return 1.0;
        }
        var maxLength = Math.Max(a.Length, b.Length);
        if (maxLength == 0)
        {
            return 1.0;
        }
        return 1.0 - (double)LevenshteinDistance(a, b) / maxLength;
    }

    private static int LevenshteinDistance(string a, string b)
    {
        var distances = new int[a.Length + 1, b.Length + 1];
        for (var i = 0; i <= a.Length; i++)
        {
            distances[i, 0] = i;
        }
        for (var j = 0; j <= b.Length; j++)
        {
            distances[0, j] = j;
        }
        for (var i = 1; i <= a.Length; i++)
        {
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                distances[i, j] = Math.Min(Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1), distances[i - 1, j - 1] + cost);
            }
        }
        return distances[a.Length, b.Length];
    }
}
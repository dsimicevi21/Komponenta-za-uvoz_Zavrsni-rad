using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ImportComponent.Core;

namespace ImportComponent.Readers;

public sealed class CsvDataReader : IDataReader
{
    public string FormatId => "csv";

    public bool CanHandle(string fileName, byte[] sample)
    {
        if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        { 
            return false; 
        }

        var firstLine = Encoding.UTF8.GetString(sample).Split('\n').FirstOrDefault() ?? string.Empty;
        return firstLine.Contains(",") || firstLine.Contains(";");
    }

    public SourceSchema InspectStructure(Stream source)
    {
        using var reader = new StreamReader(source, leaveOpen: true);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
       
        csv.Read();
        csv.ReadHeader();
        var fieldNames = csv.HeaderRecord ?? Array.Empty<string>();

        return new SourceSchema
        {
            FieldNames = fieldNames,
            EntityNames = new[] { "Default" },
        };
    }

    public IEnumerable<Record> ReadAll(Stream source, ReaderOptions options)
    {
        var encoding = Encoding.GetEncoding(options.Encoding);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = options.Delimiter ?? ",",
            HasHeaderRecord = options.HasHeaderRow,
        };

        using var reader = new StreamReader(source, encoding, leaveOpen: true);
        using var csv = new CsvReader(reader, config);

        if (options.HasHeaderRow)
        {
            while (csv.Read())
            {
                var columnCount = csv.Parser.Record?.Length ?? 0;
                var positionalNames = Enumerable.Range(0, columnCount).Select(i => $"Field{i + 1}").ToArray();
                yield return RowToRecord(csv, positionalNames);
            }
            yield break;
        }
        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? Array.Empty<string>();

        while (csv.Read())
        {
            yield return RowToRecord(csv, headers);
        }
    }
    
    private static Record RowToRecord(CsvReader csv, IReadOnlyList<string> fieldNames)
    {
        var record = new Record();
        for (var i = 0; i < fieldNames.Count; i++)
        {
            record[fieldNames[i]] = csv.GetField(i);
        }
        return record;
    }
}

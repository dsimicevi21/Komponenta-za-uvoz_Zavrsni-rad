using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using ImportComponent.Core;

namespace ImportComponent.Readers;

public sealed class JsonDataReader : IDataReader
{
    public string FormatId => "json";

    public bool CanHandle(string fileName, byte[] sample)
    {
        if (fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var trimmed = Encoding.UTF8.GetString(sample).TrimStart();
        return trimmed.Length > 0 && (trimmed[0] == '{' || trimmed[0] == '[');
    }

    public SourceSchema InspectStructure(Stream source)
    {
        using var document = JsonDocument.Parse(source);
        var root = document.RootElement;

        if (root.ValueKind == JsonValueKind.Array)
        {
            return new SourceSchema
            {
                FieldNames = FirstObjectFieldNames(root),
                EntityNames = new[] { "default" },
            };
        }

        if (root.ValueKind == JsonValueKind.Object)
        {
            var entityNames = root.EnumerateObject()
                .Where(p => p.Value.ValueKind == JsonValueKind.Array)
                .Select(p => p.Name)
                .ToArray();

            var firstArray = entityNames.Length > 0 ? root.GetProperty(entityNames[0]) : default;

            return new SourceSchema
            {
                FieldNames = entityNames.Length > 0 ? FirstObjectFieldNames(firstArray) : Array.Empty<string>(),
                EntityNames = entityNames,
            };
        }
        return new SourceSchema();
    }

    private static string[] FirstObjectFieldNames(JsonElement array)=>
        array.GetArrayLength() > 0 && array[0].ValueKind == JsonValueKind.Object
            ? array[0].EnumerateObject().Select(p => p.Name).ToArray()
            : Array.Empty<string>();

    public IEnumerable<Record> ReadAll(Stream source, ReaderOptions options)
    {
        using var buffer = new MemoryStream();
        source.CopyTo(buffer);
        var jsonBytes = buffer.ToArray();
        var arrayPropertyName = string.IsNullOrempty(options.EntityName) || options.EntityName == "default" ? null : options.EntityName;
        return ReadRecords(jsonBytes, arrayPropertyName);
    }
    private static List<Record> ReadRecords(byte[] jsonBytes, string? arrayPropertyName)
    {
        var records = new List<Record>();
        var reader = new Utf8JsonReader(jsonBytes, isFinalBlock: true, state: default);

        if (!reader.Read())
        {
            return records;
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            ReadArrayInto(ref reader, records);
            return records;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            return records;
        }

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var propertyName = reader.GetString();
            reader.Read();

            var isTargetArray = reader.TokenType == JsonTokenType.StartArray && (arrayPropertyName == null || propertyName == arrayPropertyName);

            if(isTargetArray)
            {
                ReadArrayInto(ref reader, records);
                break;
            }
                reader.Skip();
        }
        return records;
    }
    private static void ReadArrayInto(ref Utf8JsonReader reader, List<Record> records)
    {
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                records.Add(ReadObject(ref reader));
            }
            else 
            {
                reader.Skip();
            }
        }
    }

    private static Record ReadObject(ref Utf8JsonReader reader)
    {
        var record = new Record();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var propertyName = reader.GetString();
            reader.Read();
            record[propertyName] = ReadValue(ref reader);
        }
        return record;
    }

    private static object? ReadValue(ref Utf8JsonReader reader)
    {
       switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.Number:
                if (reader.TryGetInt64(out var longValue))
                {
                    return longValue;
                }
                return reader.GetDouble();
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.StartObject:
                return ReadObject(ref reader);
            case JsonTokenType.StartArray:
                var items = new List<object?>();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    items.Add(ReadValue(ref reader));
                }
                return items;
            default:
                return null;
        }
    }
}

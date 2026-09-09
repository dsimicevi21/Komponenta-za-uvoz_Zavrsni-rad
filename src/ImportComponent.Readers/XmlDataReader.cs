using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using ImportComponent.Core;
using ReaderOptions = ImportComponent.Core.ReaderOptions;

namespace ImportComponent.Readers;

public sealed class XmlDataReader : IDataReader
{
    public string FormatId => "xml";

    public bool CanHandle(string fileName, byte[] sample)
    {
        if (!fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        var trimmed = Encoding.UTF8.GetString(sample).TrimStart();
        return trimmed.StartsWith("<");
    }
    public SourceSchema InspectStructure(Stream source, string? entityName = null)
    {
        var document = XDocument.Load(source);
        var root = document.Root;
        if (root is null)
        {
            return new SourceSchema();
        }

        var rootChildren = root.Elements().ToList();
        if (IsHomogenous(rootChildren))
        {
            return new SourceSchema { FieldNames = FIeldNamesOf(rootChildren.FirstOrDefault()), EntityNames = new[] { "default" } };
        }

        var entityNames = rootChildren.Select(e => e.Name.LocalName).ToArray();
        var targetName = !string.IsNullOrEmpty(entityName) && entityNames.Contains(entityName)
            ? entityName
            : entityNames.FirstOrDefault();

        var firstItem = rootChildren.FirstOrDefault(e => e.Name.LocalName == targetName)?.Elements().FirstOrDefault();

        return new SourceSchema { FieldNames = FIeldNamesOf(firstItem), EntityNames = entityNames };
    }

    private static bool IsHomogenous(IReadOnlyCollection<XElement> elements)=>
        elements.Count > 0 && elements.Select(e => e.Name.LocalName).Distinct().Count() == 1;

    private static string[] FIeldNamesOf(XElement? element) =>
        element?.Elements().Select(e => e.Name.LocalName).ToArray() ?? Array.Empty<string>();

    public IEnumerable<Record> ReadAll(Stream source, ReaderOptions options)
    {
        using var buffer = new MemoryStream();
        source.CopyTo(buffer);
        var xmlBytes = buffer.ToArray();
        var entityName = string.IsNullOrEmpty(options.EntityName) || options.EntityName == "default" ? null : options.EntityName;

        return ReadRecords(xmlBytes, entityName);
    }

    private static IEnumerable<Record> ReadRecords(byte[] xmlBytes, string? entityName)
    {
        var containerName = entityName ?? DetectDefaultContainerName(xmlBytes);

        using var stream = new MemoryStream(xmlBytes);
        using var reader = XmlReader.Create(stream, ReaderSettings());

        reader.MoveToContent();
        if (reader.NodeType != XmlNodeType.Element)
        {
            yield break;
        }
        if (containerName is not null && !reader.ReadToDescendant(containerName))
        {
            yield break;

        }
        if (reader.IsEmptyElement)
        {
            yield break;
        }

        reader.ReadStartElement();

        while (reader.NodeType == XmlNodeType.Element)
        {
            yield return ReadItemAsRecord(reader);
        }
    }

    private static string? DetectDefaultContainerName(byte[] xmlBytes)
    {
        using var stream = new MemoryStream(xmlBytes);
        using var reader = XmlReader.Create(stream, ReaderSettings());

        reader.MoveToContent();
        if (reader.NodeType != XmlNodeType.Element || reader.IsEmptyElement)
        {
            return null;
        }
       
        reader.ReadStartElement();

        var childNames = new List<string>();
        while (reader.NodeType == XmlNodeType.Element)
        {
            childNames.Add(reader.LocalName);
            reader.Skip();
        }
        return childNames.Distinct().Count() == 1 ? null : childNames.FirstOrDefault();
    }

    private static Record ReadItemAsRecord(XmlReader reader)
    {
        var record = new Record();
        if (reader.IsEmptyElement)
        {
            reader.Read();
            return record;
        }

        reader.ReadStartElement();

        while (reader.NodeType == XmlNodeType.Element)
        {
            var fieldName = reader.LocalName;
            record[fieldName] = reader.ReadElementContentAsString();
        }

        reader.ReadEndElement();
        return record;
    }
    private static XmlReaderSettings ReaderSettings() =>
        new ()
        {
            IgnoreWhitespace = true,
            DtdProcessing = DtdProcessing.Prohibit,
        };
}

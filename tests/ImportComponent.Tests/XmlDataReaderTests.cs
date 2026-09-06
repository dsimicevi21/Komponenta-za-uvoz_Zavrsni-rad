using System.Text;
using ImportComponent.Core;
using ImportComponent.Readers;

namespace ImportComponent.Tests;

public class XmlDataReaderTests
{
    private const string MultiEntityXml = """
        <data>
          <students>
            <student><first_name>Ana</first_name><year>1</year></student>
            <student><first_name>Marko</first_name><year>2</year></student>
          </students>
          <courses>
            <course><code>UP1</code><ects>6</ects></course>
          </courses>
        </data>
        """;

    private const string SingleEntityXml = """
        <students>
          <student><first_name>Ana</first_name><year>1</year></student>
          <student><first_name>Marko</first_name><year>2</year></student>
        </students>
        """;

    [Fact]
    public void CanHandle_XmlExtensionAndElementContent_ReturnsTrue()
    {
        var reader = new XmlDataReader();
        var sample = Encoding.UTF8.GetBytes(MultiEntityXml);

        Assert.True(reader.CanHandle("students.xml", sample));
    }

    [Fact]
    public void CanHandle_WrongExtension_ReturnsFalse()
    {
        var reader = new XmlDataReader();
        var sample = Encoding.UTF8.GetBytes(MultiEntityXml);

        Assert.False(reader.CanHandle("students.json", sample));
    }

    [Fact]
    public void InspectStructure_HeterogeneousRoot_ReturnsEntityNamesForEachContainer()
    {
        var reader = new XmlDataReader();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(MultiEntityXml));

        var schema = reader.InspectStructure(stream);

        Assert.Equal(new[] { "students", "courses" }, schema.EntityNames);
        Assert.Equal(new[] { "first_name", "year" }, schema.FieldNames);
    }

    [Fact]
    public void InspectStructure_HomogeneousRoot_ReturnsSingleDefaultEntity()
    {
        var reader = new XmlDataReader();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SingleEntityXml));

        var schema = reader.InspectStructure(stream);

        Assert.Equal(new[] { "default" }, schema.EntityNames);
        Assert.Equal(new[] { "first_name", "year" }, schema.FieldNames);
    }

    [Fact]
    public void ReadAll_SelectedEntity_ReturnsOnlyThatEntitysRecords()
    {
        var reader = new XmlDataReader();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(MultiEntityXml));

        var records = reader.ReadAll(stream, new ReaderOptions { EntityName = "courses" }).ToList();

        Assert.Single(records);
        Assert.Equal("UP1", records[0]["code"]);
        Assert.Equal("6", records[0]["ects"]);
    }

    [Fact]
    public void ReadAll_DefaultEntity_OnHeterogeneousRoot_ReturnsFirstContainer()
    {
        var reader = new XmlDataReader();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(MultiEntityXml));

        var records = reader.ReadAll(stream, new ReaderOptions()).ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal("Ana", records[0]["first_name"]);
    }

    [Fact]
    public void ReadAll_HomogeneousRoot_ReturnsAllItems()
    {
        var reader = new XmlDataReader();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SingleEntityXml));

        var records = reader.ReadAll(stream, new ReaderOptions()).ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal("Marko", records[1]["first_name"]);
    }
}
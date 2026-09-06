using System.Text;
using ImportComponent.Core;
using ImportComponent.Readers;

namespace ImportComponent.Tests;

public class JsonDataReaderTests
{
    private const string MultiEntityJson = """
        {
          "students": [
            { "first_name": "Ana", "email": "ana@example.com", "year": 1 },
            { "first_name": "Marko", "email": "marko@example.com", "year": 2 }
          ],
          "courses": [
            { "code": "UP1", "ects": 6 }
          ]
        }
        """;

    private const string ArrayOnlyJson = """
        [
          { "first_name": "Ana", "year": 1 },
          { "first_name": "Marko", "year": 2 }
        ]
        """;

    [Fact]
    public void CanHandle_JsonExtensionAndObjectContent_ReturnsTrue()
    {
        var reader = new JsonDataReader();
        var sample = Encoding.UTF8.GetBytes(MultiEntityJson);

        Assert.True(reader.CanHandle("students.json", sample));
    }

    [Fact]
    public void CanHandle_WrongExtension_ReturnsFalse()
    {
        var reader = new JsonDataReader();
        var sample = Encoding.UTF8.GetBytes(MultiEntityJson);

        Assert.False(reader.CanHandle("students.csv", sample));
    }

    [Fact]
    public void InspectStructure_ObjectRoot_ReturnsEntityNamesForEachArrayProperty()
    {
        var reader = new JsonDataReader();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(MultiEntityJson));

        var schema = reader.InspectStructure(stream);

        Assert.Equal(new[] { "students", "courses" }, schema.EntityNames);
        Assert.Equal(new[] { "first_name", "email", "year" }, schema.FieldNames);
    }

    [Fact]
    public void ReadAll_SelectedEntity_ReturnsOnlyThatEntitysRecords()
    {
        var reader = new JsonDataReader();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(MultiEntityJson));

        var records = reader.ReadAll(stream, new ReaderOptions { EntityName = "courses" }).ToList();

        Assert.Single(records);
        Assert.Equal("UP1", records[0]["code"]);
        Assert.Equal(6L, records[0]["ects"]);
    }

    [Fact]
    public void ReadAll_DefaultEntity_ReturnsFirstArrayProperty()
    {
        var reader = new JsonDataReader();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(MultiEntityJson));

        var records = reader.ReadAll(stream, new ReaderOptions()).ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal("Ana", records[0]["first_name"]);
        Assert.Equal(1L, records[0]["year"]);
    }

    [Fact]
    public void ReadAll_ArrayRoot_ReturnsAllRecords()
    {
        var reader = new JsonDataReader();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(ArrayOnlyJson));

        var records = reader.ReadAll(stream, new ReaderOptions()).ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal("Marko", records[1]["first_name"]);
    }
}
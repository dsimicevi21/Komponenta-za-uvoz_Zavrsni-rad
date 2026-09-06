using System.Text;
using ImportComponent.Core;
using ImportComponent.Readers;

namespace ImportComponent.Tests;

public class CsvDataReaderTests
{
    private const string SampleCsv =
        "first_name,last_name,email,year\nAna,Anic,ana@example.com,1\nMarko,Markovic,marko@example.com,2\n";

    [Fact]
    public void CanHandle_CsvExtensionAndCommaContent_ReturnsTrue()
    {
        var reader = new CsvDataReader();
        var sample = Encoding.UTF8.GetBytes(SampleCsv);

        Assert.True(reader.CanHandle("students.csv", sample));
    }

    [Fact]
    public void CanHandle_WrongExtension_ReturnsFalse()
    {
        var reader = new CsvDataReader();
        var sample = Encoding.UTF8.GetBytes(SampleCsv);

        Assert.False(reader.CanHandle("students.json", sample));
    }

    [Fact]
    public void InspectStructure_ReturnsHeaderFieldNames()
    {
        var reader = new CsvDataReader();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SampleCsv));

        var schema = reader.InspectStructure(stream);

        Assert.Equal(new[] { "first_name", "last_name", "email", "year" }, schema.FieldNames);
    }

    [Fact]
    public void ReadAll_StreamsAllRowsAsRecords()
    {
        var reader = new CsvDataReader();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SampleCsv));

        var records = reader.ReadAll(stream, new ReaderOptions()).ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal("Ana", records[0]["first_name"]);
        Assert.Equal("marko@example.com", records[1]["email"]);
    }
}
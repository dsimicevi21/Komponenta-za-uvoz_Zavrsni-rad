using Record = ImportComponent.Core.Record;

namespace ImportComponent.Tests;

public class RecordTests
{
    [Fact]
    public void Indexer_SetAndGet_ReturnsStoredValue()
    {
        var record = new Record();
        record["name"] = "Ana";

        Assert.Equal("Ana", record["name"]);
    }

    [Fact]
    public void Indexer_MissingField_ReturnsNull()
    {
        var record = new Record();

        Assert.Null(record["missing"]);
    }
}
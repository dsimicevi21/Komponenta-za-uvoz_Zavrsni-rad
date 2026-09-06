using ImportComponent.BLL;
using Record = ImportComponent.Core.Record;

namespace ImportComponent.Tests;

public class FieldMapperTests
{
    [Fact]
    public void Map_RenamesFieldsAccordingToMapping()
    {
        var source = new Record
        {
            ["first_name"] = "Ana",
            ["email"] = "ana@example.com",
        };

        var mapping = new Dictionary<string, string>
        {
            ["first_name"] = "first_name",
            ["email"] = "email",
        };

        var mapped = FIeldMapper.Map(source, mapping);

        Assert.Equal("Ana", mapped["first_name"]);
        Assert.Equal("ana@example.com", mapped["email"]);
    }

    [Fact]
    public void Map_UnmappedSourceFields_AreIgnored()
    {
        var source = new Record
        {
            ["first_name"] = "Ana",
            ["internal_note"] = "not mapped",
        };

        var mapping = new Dictionary<string, string> { ["first_name"] = "first_name" };

        var mapped = FIeldMapper.Map(source, mapping);

        Assert.Single(mapped.Fields);
        Assert.Null(mapped["internal_note"]);
    }
}
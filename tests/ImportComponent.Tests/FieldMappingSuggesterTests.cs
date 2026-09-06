using ImportComponent.BLL;
using ImportComponent.Core;

namespace ImportComponent.Tests;

public class FieldMappingSuggesterTests
{
    private static List<ColumnInfo> StudentColumns => new()
    {
        new() { Name = "first_name", DataType = "text", IsNullable = false },
        new() { Name = "last_name", DataType = "text", IsNullable = false },
        new() { Name = "email", DataType = "text", IsNullable = false },
        new() { Name = "year", DataType = "integer", IsNullable = true },
    };

    [Fact]
    public void Suggest_ExactNameMatch_IsSuggested()
    {
        var suggestions = FieldMappingSuggester.Suggest(new[] { "email" }, StudentColumns);

        Assert.Equal("email", suggestions["email"]);
    }

    [Fact]
    public void Suggest_DifferentCasingAndSeparators_StillMatches()
    {
        var suggestions = FieldMappingSuggester.Suggest(new[] { "First Name", "LastName" }, StudentColumns);

        Assert.Equal("first_name", suggestions["First Name"]);
        Assert.Equal("last_name", suggestions["LastName"]);
    }

    [Fact]
    public void Suggest_NoSimilarTarget_IsNotSuggested()
    {
        var suggestions = FieldMappingSuggester.Suggest(new[] { "completely_unrelated_field" }, StudentColumns);

        Assert.False(suggestions.ContainsKey("completely_unrelated_field"));
    }

    [Fact]
    public void Suggest_NeverAssignsTwoSourceFieldsToSameTarget()
    {
        var suggestions = FieldMappingSuggester.Suggest(new[] { "email", "e_mail" }, StudentColumns);

        var targets = suggestions.Values.ToList();
        Assert.Equal(targets.Count, targets.Distinct().Count());
    }
}
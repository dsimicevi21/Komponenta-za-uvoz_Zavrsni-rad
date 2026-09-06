using ImportComponent.BLL;
using ImportComponent.Core;
using Record = ImportComponent.Core.Record;

namespace ImportComponent.Tests;

public class RecordValidatorTests
{
    [Fact]
    public void Validate_AllRequiredColumnsPresent_IsValid()
    {
        var validator = new RecordValidator();
        var record = new Record { ["first_name"] = "Ana" };
        var columns = new List<ColumnInfo>
        {
            new() { Name = "first_name", DataType = "text", IsNullable = false },
        };

        var result = validator.Validate(record, columns);

        Assert.True(result.IsValid);
        Assert.Equal("Ana", result.Record!["first_name"]);
    }

    [Fact]
    public void Validate_MissingRequiredColumn_IsInvalid()
    {
        var validator = new RecordValidator();
        var record = new Record();
        var columns = new List<ColumnInfo>
        {
            new() { Name = "first_name", DataType = "text", IsNullable = false },
        };

        var result = validator.Validate(record, columns);

        Assert.False(result.IsValid);
        Assert.NotNull(result.RejectionReason);
    }

    [Fact]
    public void Validate_MissingNullableColumn_IsValid()
    {
        var validator = new RecordValidator();
        var record = new Record();
        var columns = new List<ColumnInfo>
        {
            new() { Name = "year", DataType = "integer", IsNullable = true },
        };

        var result = validator.Validate(record, columns);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_MissingColumnWithDefault_IsValid()
    {
        var validator = new RecordValidator();
        var record = new Record();
        var columns = new List<ColumnInfo>
        {
            new() { Name = "id", DataType = "integer", IsNullable = false, HasDefault = true },
        };

        var result = validator.Validate(record, columns);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_IntegerColumn_CoercesStringToInt()
    {
        var validator = new RecordValidator();
        var record = new Record { ["year"] = "2" };
        var columns = new List<ColumnInfo>
        {
            new() { Name = "year", DataType = "integer", IsNullable = true },
        };

        var result = validator.Validate(record, columns);

        Assert.True(result.IsValid);
        Assert.Equal(2, result.Record!["year"]);
    }

    [Fact]
    public void Validate_IntegerColumn_NonNumericValue_IsInvalid()
    {
        var validator = new RecordValidator();
        var record = new Record { ["year"] = "not-a-number" };
        var columns = new List<ColumnInfo>
        {
            new() { Name = "year", DataType = "integer", IsNullable = true },
        };

        var result = validator.Validate(record, columns);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_BooleanColumn_CoercesStringToBool()
    {
        var validator = new RecordValidator();
        var record = new Record { ["in_stock"] = "true" };
        var columns = new List<ColumnInfo>
        {
            new() { Name = "in_stock", DataType = "boolean", IsNullable = false },
        };

        var result = validator.Validate(record, columns);

        Assert.True(result.IsValid);
        Assert.Equal(true, result.Record!["in_stock"]);
    }

    [Fact]
    public void Validate_BooleanColumn_InvalidValue_IsInvalid()
    {
        var validator = new RecordValidator();
        var record = new Record { ["in_stock"] = "maybe" };
        var columns = new List<ColumnInfo>
        {
            new() { Name = "in_stock", DataType = "boolean", IsNullable = false },
        };

        var result = validator.Validate(record, columns);

        Assert.False(result.IsValid);
    }
    [Fact]
    public void Validate_NumericColumn_CoercesStringToDecimal()
    {
        var validator = new RecordValidator();
        var record = new Record { ["price"] = "19.99" };
        var columns = new List<ColumnInfo>
        {
            new() { Name = "price", DataType = "numeric", IsNullable = true },
        };

        var result = validator.Validate(record, columns);

        Assert.True(result.IsValid);
        Assert.Equal(19.99m, result.Record!["price"]);
    }
}
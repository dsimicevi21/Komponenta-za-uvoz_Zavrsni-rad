using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using ImportComponent.Core;
using System.Reflection.Metadata;

namespace ImportComponent.BLL;

public sealed class RecordValidator
{
    public ValidationResult Validate(Record record, IReadOnlyList<ColumnInfo> targetColumns)
    {
        var coerced = new Record();

        foreach (var column in targetColumns)
        {
            var hasValue = record.Fields.TryGetValue(column.Name, out var rawValue) && rawValue is not null;

            if (!hasValue)
            {
                var canBeOmitted = column.IsNullable || column.HasDefault;
                if (!canBeOmitted)
                {
                    return ValidationResult.Invalid($"Missing required value for column '{column.Name}'.");
                }
                continue;
            }

            if (!TryCoerce(rawValue, column.DataType, out var coercedValue))
            {
                return ValidationResult.Invalid($"Value '{rawValue}' for column '{column.Name}' is not a valid '{column.DataType}'.");
            }

            coerced[column.Name] = coercedValue;
        }
        return ValidationResult.Valid(coerced);
    }

    private static bool TryCoerce(object rawValue, string dataType, out object? coercedValue)
    {
        var text = rawValue as string ?? Convert.ToString (rawValue, CultureInfo.InvariantCulture) ?? string.Empty;

        switch (dataType)
        {
            case "integer":
            case "smallint":
                if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
                {
                    coercedValue = intValue;
                    return true;
                }

                break;

                case "bigint":
                if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longValue))
                {
                    coercedValue = longValue;
                    return true;
                }
                break;

            case "numeric":
            case "real":
            case "double precision":
                if (decimal.TryParse(text, NumberStyles.Number | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var decimalValue))
                {
                    coercedValue = decimalValue;
                    return true;
                }
                break;

            case "boolean":
                if(bool.TryParse(text, out var boolValue))
                {
                    coercedValue = boolValue;
                    return true;
                }
                break;
            case "date":
            case "timestamp without time zone":
            case "timestamp with time zone":
                if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateValue))
                {
                    coercedValue = dateValue;
                    return true;
                }
                break;

            default:
                coercedValue = text;
                return true;
        }
        coercedValue = null;
        return false;
    }
}

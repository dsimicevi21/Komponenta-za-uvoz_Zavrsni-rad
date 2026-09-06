using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImportComponent.Core;

namespace ImportComponent.BLL;

public static class FIeldMapper
{
    public static Record Map(Record source, IReadOnlyDictionary<string, string> sourceToTargetField)
    {
        var mapped = new Record();
        foreach (var (sourceField, targetField) in sourceToTargetField)
        {
            mapped[targetField] = source[sourceField];
        }
        return mapped;
    }
}

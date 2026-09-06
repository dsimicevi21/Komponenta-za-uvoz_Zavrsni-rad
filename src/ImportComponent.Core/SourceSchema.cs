using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportComponent.Core
{
    public sealed class SourceSchema
    {
        public IReadOnlyList<string> FieldNames { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> EntityNames { get; init; } = Array.Empty<string>();
    }
}

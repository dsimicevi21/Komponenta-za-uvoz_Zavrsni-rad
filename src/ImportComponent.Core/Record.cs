using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportComponent.Core
{
    public sealed class Record
    {
        public IDictionary<string, object?> Fields { get; } = new Dictionary<string, object?>();

        public object? this[string fieldName]
        {
            get => Fields.TryGetValue(fieldName, out var value) ? value : null;
            set => Fields[fieldName] = value;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportComponent.Core
{
    public sealed class ColumnInfo
    {
        public required string Name { get; init; }
        public required string DataType { get; init; }
        public bool IsNullable { get; init;}
        public bool HasDefault { get; init; }
    }
}

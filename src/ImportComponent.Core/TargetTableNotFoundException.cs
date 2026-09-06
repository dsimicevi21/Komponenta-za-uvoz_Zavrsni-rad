using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportComponent.Core
{
    public sealed class TargetTableNotFoundException : Exception
    {
        public TargetTableNotFoundException(string tableName)
            : base($"Target table '{tableName}' not found.")
        {
        }
    }
}

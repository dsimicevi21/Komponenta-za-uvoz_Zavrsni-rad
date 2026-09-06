using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportComponent.Core
{
    public sealed class ReaderOptions
    {
        public string? Delimiter { get; set; }
        public string Encoding { get; set; }
        public bool HasHeaderRow { get; set; } = true;
        public string? EntityName { get; set; }
    }
}

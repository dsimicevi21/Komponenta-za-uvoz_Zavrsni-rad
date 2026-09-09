using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ImportComponent.Core
{
    public interface IDataReader
    {
        string FormatId { get; }
        bool CanHandle(string fileName, byte[] sample);
        SourceSchema InspectStructure(Stream source, string? entityName = null);

        IEnumerable<Record> ReadAll(Stream source, ReaderOptions options);
    }
}

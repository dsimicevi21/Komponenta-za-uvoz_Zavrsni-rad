using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImportComponent.Core;

namespace ImportComponent.Readers;

public sealed class DataReaderRegistry : IDataReaderRegistry
{
    private readonly IReadOnlyList<IDataReader> _readers;

    public DataReaderRegistry(IEnumerable<IDataReader> readers)
    {
        _readers = readers.ToList();
    }

    public IDataReader? FindReader(string fileName, byte[] sample) =>
                _readers.FirstOrDefault(reader => reader.CanHandle(fileName, sample));
}

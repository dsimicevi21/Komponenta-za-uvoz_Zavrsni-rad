using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportComponent.Core
{
    public interface IDataReaderRegistry
    {
        IDataReader? FindReader(string fileName, byte[] sample);
    }
}

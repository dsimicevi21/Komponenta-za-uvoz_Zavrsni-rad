using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportComponent.Core
{
    public interface ITargetSystemProvider
    {
        string SystemID { get; }
        ISchemaIntrospector CreateIntrospector();
        IDataWriterFactory CreateWriterFactory();
    }
}

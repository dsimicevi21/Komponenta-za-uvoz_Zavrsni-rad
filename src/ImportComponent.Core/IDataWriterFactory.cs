using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportComponent.Core
{
    public interface IDataWriterFactory
    {
        string TargetSystemId { get; }
        IDataWriter CreateWriter(string targetTable);

        int CleanupOrphanedStaging(TimeSpan olderThan);
    }
}

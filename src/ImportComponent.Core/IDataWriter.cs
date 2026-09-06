using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportComponent.Core
{
    public interface IDataWriter
    {
        void WriteToStaging(string runId, IEnumerable<Record> records);

        ImportSummary CommitStaging(string runID);

        void DiscardStaging(string runId);
    }
}

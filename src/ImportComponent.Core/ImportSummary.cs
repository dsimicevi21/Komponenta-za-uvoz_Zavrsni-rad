using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportComponent.Core;

public sealed class ImportSummary
{
    public required string RunId { get; init; }
    public int InsertedCount { get; init; }
    public int RejectedCount { get; init; }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImportComponent.Core;

namespace ImportComponent.BLL;

public sealed class ImportRunResult
{
    public required string RunId { get; init; }
    public required string TargetTable { get; init; }

    public int ValidCount { get; init; }

    public IReadOnlyList<RejectedRecord> RejectedRecords { get; init; } = Array.Empty<RejectedRecord>();

    public IReadOnlyList<Record> ValidRecords { get; init; } = Array.Empty<Record>();
}

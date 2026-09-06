using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportComponent.Core;

public sealed class RejectedRecord
{
    public required Record record { get; init; }
    public required string Reason { get; init; }
}

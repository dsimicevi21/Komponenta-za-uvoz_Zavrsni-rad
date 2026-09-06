using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImportComponent.Core;

namespace ImportComponent.BLL;

public sealed class ValidationResult
{
    public bool Isvalid { get; private init; }
    public string? RejectionReason { get; private init; }
    public Record? Record { get; private init; }

    public static ValidationResult Valid(Record record) => new() {  Isvalid = true , Record = record };

    public static ValidationResult Invalid(string reason) => new() { Isvalid = false , RejectionReason = reason };
}

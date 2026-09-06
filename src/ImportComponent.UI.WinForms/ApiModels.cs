using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace ImportComponent.UI.WinForms;

public sealed record TargetSystemDto(string SystemId);

public sealed record TableDto(string Name);

public sealed record ColumnDto(string Name, string DataType, bool IsNullable, bool HasDefault);

public sealed record InspectResultDto(string FormatId, List<string> FieldNames, List<string> EntityNames);

public sealed record RejectedRecordDto(string Reason);

public sealed record RunImportResponse(
    string RunId,
    string TargetTable,
    int ValidCount,
    List<RejectedRecordDto> RejectedRecords,
    List<Dictionary<string, JsonElement>> ValidRecords);

public sealed record ImportSummaryDto(string RunId, int InsertedCount);
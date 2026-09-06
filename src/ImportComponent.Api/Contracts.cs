namespace ImportComponent.Api.Contracts;

public sealed record TargetSystemDto(string SystemId);

public sealed record TableDto(string Name);

public sealed record ColumnDto(string Name, string DataType, bool IsNullable, bool HasDefault);

public sealed record InspectResultDto(string FormatId, IReadOnlyList<string> FieldNames, IReadOnlyList<string> EntityNames);

public sealed record SuggestMappingRequest(IReadOnlyList<string> SourceFieldNames, string TargetSystem, string TargetTable);

public sealed record RejectedRecordDto(string Reason);

public sealed record RunImportResponse(
    string RunId,
    string TargetTable,
    int ValidCount,
    IReadOnlyList<RejectedRecordDto> RejectedRecords,
    IReadOnlyList<IReadOnlyDictionary<string,object?>> ValidRecords);
public sealed record ImportSummaryDto(string RunId, int InsertedCount);

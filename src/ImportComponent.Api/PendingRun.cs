namespace ImportComponent.Api;

public sealed record PendingRun(string RunId, string TargetSystem, string TargetTable);

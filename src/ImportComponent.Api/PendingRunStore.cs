using System.Collections.Concurrent;

namespace ImportComponent.Api;
public sealed class PendingRunStore
{
    private readonly ConcurrentDictionary<string, PendingRun> _runs = new();
    public void Add(PendingRun run) => _runs[run.RunId] = run;

    public PendingRun? Get(string runId) => _runs.TryGetValue(runId, out var run) ? run : null;

    public void Remove(string runId) => _runs.TryRemove(runId, out _);
}
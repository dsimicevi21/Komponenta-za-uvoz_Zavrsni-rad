using ImportComponent.Core;

namespace ImportComponent.Tests;

public class StagingNamingTests
{
    [Fact]
    public void NewRunId_EmbeddedInStagingName_ParsesBackToApproximatelyNow()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-2);
        var runId = StagingNaming.NewRunId();
        var after = DateTimeOffset.UtcNow.AddSeconds(2);

        var stagingName = "staging_" + runId;

        var parsed = StagingNaming.TryParseCreatedAt(stagingName, out var createdAt);

        Assert.True(parsed);
        Assert.InRange(createdAt, before, after);
    }

    [Fact]
    public void TryParseCreatedAt_NameWithoutStagingPrefix_ReturnsFalse()
    {
        var parsed = StagingNaming.TryParseCreatedAt("students", out _);

        Assert.False(parsed);
    }

    [Fact]
    public void TryParseCreatedAt_MalformedTimestamp_ReturnsFalse()
    {
        var parsed = StagingNaming.TryParseCreatedAt("staging_not-a-timestamp", out _);

        Assert.False(parsed);
    }
}
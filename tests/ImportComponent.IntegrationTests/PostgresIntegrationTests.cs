using ImportComponent.Core;
using ImportComponent.DAL.Postgres;
using Npgsql;
using Record = ImportComponent.Core.Record;

namespace ImportComponent.IntegrationTests;
public class PostgresIntegrationTests
{
    [Fact]
    public void ListTables_IncludesAFreshlyCreatedTable()
    {
        var tableName = CreateScratchTable();
        try
        {
            var introspector = new PostgresSchemaIntroSpector(TestConfig.PostgresConnectionString);

            var tables = introspector.ListTables();

            Assert.Contains(tables, t => t.Name == tableName);
        }
        finally
        {
            DropTable(tableName);
        }
    }

    [Fact]
    public void ListColumns_ReturnsRealColumnMetadata()
    {
        var tableName = CreateScratchTable();
        try
        {
            var introspector = new PostgresSchemaIntroSpector(TestConfig.PostgresConnectionString);

            var columns = introspector.ListColumns(tableName);

            var id = Assert.Single(columns, c => c.Name == "id");
            Assert.False(id.IsNullable);
            Assert.True(id.HasDefault); 

            var name = Assert.Single(columns, c => c.Name == "name");
            Assert.False(name.IsNullable);
            Assert.False(name.HasDefault);

            var score = Assert.Single(columns, c => c.Name == "score");
            Assert.True(score.IsNullable);
        }
        finally
        {
            DropTable(tableName);
        }
    }

    [Fact]
    public void ListColumns_UnknownTable_ThrowsTargetTableNotFoundException()
    {
        var introspector = new PostgresSchemaIntroSpector(TestConfig.PostgresConnectionString);

        Assert.Throws<TargetTableNotFoundException>(() => introspector.ListColumns("does_not_exist_" + Guid.NewGuid().ToString("N")));
    }

    [Fact]
    public void WriteToStaging_ThenCommit_RowsLandInTargetTable_AndStagingIsDropped()
    {
        var tableName = CreateScratchTable();
        try
        {
            var writer = new PostgresDataWriter(TestConfig.PostgresConnectionString, tableName);
            var runId = Guid.NewGuid().ToString("N");

            var records = new List<Record>
            {
                new() { ["name"] = "Ana", ["score"] = 10 },
                new() { ["name"] = "Marko", ["score"] = 20 },
            };

            writer.WriteToStaging(runId, records);
            var summary = writer.CommitStaging(runId);

            Assert.Equal(2, summary.InsertedCount);
            Assert.Equal(2, CountRows(tableName));
            Assert.False(StagingTableExists(runId));
        }
        finally
        {
            DropTable(tableName);
        }
    }

    [Fact]
    public void WriteToStaging_ThenDiscard_TargetTableStaysEmpty_AndStagingIsDropped()
    {
        var tableName = CreateScratchTable();
        try
        {
            var writer = new PostgresDataWriter(TestConfig.PostgresConnectionString, tableName);
            var runId = Guid.NewGuid().ToString("N");

            writer.WriteToStaging(runId, new List<Record> { new() { ["name"] = "Ana", ["score"] = 10 } });
            writer.DiscardStaging(runId);

            Assert.Equal(0, CountRows(tableName));
            Assert.False(StagingTableExists(runId));
        }
        finally
        {
            DropTable(tableName);
        }
    }

    [Fact]
    public void WriteToStaging_TargetTableDoesNotExist_ThrowsTargetTableNotFoundException()
    {
        var writer = new PostgresDataWriter(TestConfig.PostgresConnectionString, "does_not_exist_" + Guid.NewGuid().ToString("N"));

        Assert.Throws<TargetTableNotFoundException>(
            () => writer.WriteToStaging(Guid.NewGuid().ToString("N"), new List<Record> { new() { ["name"] = "Ana" } }));
    }

    private static string CreateScratchTable()
    {
        var tableName = "it_" + Guid.NewGuid().ToString("N")[..12];

        using var connection = new NpgsqlConnection(TestConfig.PostgresConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"""
            CREATE TABLE "{tableName}" (
                id SERIAL PRIMARY KEY,
                name TEXT NOT NULL,
                score INTEGER
            )
            """;
        command.ExecuteNonQuery();

        return tableName;
    }

    private static void DropTable(string tableName)
    {
        using var connection = new NpgsqlConnection(TestConfig.PostgresConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"DROP TABLE IF EXISTS \"{tableName}\"";
        command.ExecuteNonQuery();
    }

    private static int CountRows(string tableName)
    {
        using var connection = new NpgsqlConnection(TestConfig.PostgresConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT count(*) FROM \"{tableName}\"";
        return (int)(long)command.ExecuteScalar()!;
    }

    private static bool StagingTableExists(string runId)
    {
        using var connection = new NpgsqlConnection(TestConfig.PostgresConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT to_regclass(@name) IS NOT NULL";
        command.Parameters.AddWithValue("name", "staging_" + runId);
        return (bool)command.ExecuteScalar()!;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImportComponent.Core;
using Npgsql;

namespace ImportComponent.DAL.Postgres;

internal class PostgresWriterFactory : IDataWriterFactory
{
    private readonly string _connectionString;
    public PostgresWriterFactory(string connectionString)
    {
        _connectionString = connectionString;
    }
    public string TargetSystemId => "Postgres";
    public IDataWriter CreateWriter(string targetTable) => new PostgresDataWriter(_connectionString, targetTable);
    public int CleanupOrphanedStaging(TimeSpan olderThan)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var candidateTables = new List<string>();
        using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT table_name 
                FROM information_schema.tables 
                WHERE table_type = 'BASE TABLE'
                AND table_schema NOT IN ('pg_catalog', 'information_schema')
                """;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                candidateTables.Add(reader.GetString(0));
            }
        }
        var cutoff = DateTimeOffset.UtcNow - olderThan;
        var droppedCount = 0;

        foreach (var tableName in candidateTables)
        {
            if (!StagingNaming.TryParseCreatedAt(tableName, out var createdAt) || createdAt >= cutoff)
            {
                continue;
            }

            using var dropCommand = connection.CreateCommand();
            dropCommand.CommandText = $"DROP TABLE IF EXISTS {SqlIdentifier.Quote(tableName)}";
            dropCommand.ExecuteNonQuery();
            droppedCount++;
        }
        return droppedCount;
    }
}

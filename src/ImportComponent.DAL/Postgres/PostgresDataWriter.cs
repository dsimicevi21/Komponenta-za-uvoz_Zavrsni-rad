using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImportComponent.Core;
using Npgsql;

namespace ImportComponent.DAL.Postgres;

public sealed class PostgresDataWriter : IDataWriter
{
    private readonly string _connectionString;
    private readonly string _targetTable;

    public PostgresDataWriter(string connectionString, string targetTable)
    {
        _connectionString = connectionString;
        _targetTable = targetTable;
    }

    public void WriteToStaging(string runId, IEnumerable<Record> records)
    {
        var targetTableQuoted = SqlIdentifier.Quote(_targetTable);
        var stagingTableQuoted = SqlIdentifier.Quote(SqlIdentifier.SanitizeRunId(runId));

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        if (!TableExists(connection, _targetTable))
        {
            throw new TargetTableNotFoundException(_targetTable);
        }

        using (var createCommand = connection.CreateCommand())
        {
            createCommand.CommandText = $"CREATE TABLE IF NOT EXISTS {stagingTableQuoted} (LIKE {targetTableQuoted} INCLUDING DEFAULTS)";
            createCommand.ExecuteNonQuery();
        }

        using var recordEnumerator = records.GetEnumerator();
        if (!recordEnumerator.MoveNext())
        {
            return;
        }

        var firstRecord = recordEnumerator.Current;
        var columns = firstRecord.Fields.Keys.ToList();
        if (columns.Count == 0)
        {
            return;
        }

        var columnList = string.Join(", ", columns.Select(SqlIdentifier.Quote));

        using var importer = connection.BeginBinaryImport($"COPY {stagingTableQuoted} ({columnList}) FROM STDIN (FORMAT BINARY)");

        WriteRow(importer, firstRecord, columns);
        while (recordEnumerator.MoveNext())
        {
            WriteRow(importer, recordEnumerator.Current, columns);
        }
        importer.Complete();
    }
    private static void WriteRow(NpgsqlBinaryImporter importer, Record record, IReadOnlyList<string> columns)
    {
        importer.StartRow();
        foreach (var column in columns)
        {
            var value = record.Fields.TryGetValue(column, out var fieldValue) ? fieldValue : null;

            if (value is null)
            {
                importer.WriteNull();
            }
            else
            {
                importer.Write(value);
            }
        }
    }

    public ImportSummary CommitStaging(string runId)
    {
        var targetTableQuoted = SqlIdentifier.Quote(_targetTable);
        var stagingTableQuoted = SqlIdentifier.Quote(SqlIdentifier.SanitizeRunId(runId));

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        int insertedCount;
        using (var insertCommand = connection.CreateCommand())
        {
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = $"INSERT INTO {targetTableQuoted} SELECT * FROM {stagingTableQuoted}";
            insertedCount = insertCommand.ExecuteNonQuery();
        }
        
        using (var dropCommand = connection.CreateCommand())
        {
            dropCommand.Transaction = transaction;
            dropCommand.CommandText = $"DROP TABLE {stagingTableQuoted}";
            dropCommand.ExecuteNonQuery();
        }
        transaction.Commit();

        return new ImportSummary
        {
            RunId = runId,
            InsertedCount = insertedCount,
            RejectedCount = 0,
        };
    }

    public void DiscardStaging(string runId)
    {
        var stagingTableQuoted = SqlIdentifier.Quote(SqlIdentifier.SanitizeRunId(runId));

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        using var dropCommand = connection.CreateCommand();
        dropCommand.CommandText = $"DROP TABLE IF EXISTS {stagingTableQuoted}";
        dropCommand.ExecuteNonQuery();
    }

    private static bool TableExists(NpgsqlConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT to_regclass(@tableName) IS NOT NULL";
        command.Parameters.AddWithValue("@tableName", tableName);
        return (bool)(command.ExecuteScalar() ?? false);
    }
}

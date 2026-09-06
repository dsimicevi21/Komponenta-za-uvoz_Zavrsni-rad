using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImportComponent.Core;
using Npgsql;

namespace ImportComponent.DAL.Postgres;

public sealed class PostgresSchemaIntroSpector : ISchemaIntrospector
{
    private readonly string _connectionString;

    public PostgresSchemaIntroSpector(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IReadOnlyList<TableInfo> ListTables()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        const string sql = """
            
            SELECT table_name 
            FROM information_schema.tables 
            WHERE table_type = 'BASE TABLE' AND table
            _schema NOT IN ('pg_catalog', 'information_schema') 
            ORDER BY table_name 
            
            """;

        using var command = new NpgsqlCommand(sql, connection);
        using var reader = command.ExecuteReader();

        var tables = new List<TableInfo>();
        while (reader.Read())
        {
           tables.Add(new TableInfo { Name = reader.GetString(0) });
        }
        return tables;
    }
    public IReadOnlyList<ColumnInfo> ListColumns(string tableName)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        const string sql = """
            SELECT column_name, data_type, is_nullable, column_default
            FROM information_schema.columns 
            WHERE table_name = @tableName 
            ORDER BY ordinal_position
            """;
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@tableName", tableName);
        using var reader = command.ExecuteReader();

        var columns = new List<ColumnInfo>();
        while (reader.Read())
        {
            columns.Add(new ColumnInfo
            {
                Name = reader.GetString(0),
                DataType = reader.GetString(1),
                IsNullable = reader.GetString(2) == "YES",
                HasDefault = !reader.IsDBNull(3)
            });
        }
        if ( columns.Count == 0 )
        {
            throw new TargetTableNotFoundException(tableName);
        }

        return columns;
    }
}

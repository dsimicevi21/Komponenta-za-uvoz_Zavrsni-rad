using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Npgsql;

namespace ImportComponent.UI.WinForms;

internal static class DevResetService
{
    private const string PostgresConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=import_demo";
    private const string MongoConnectionString = "mongodb://localhost:27017";
    private const string MongoDatabaseName = "import_demo";

    public static void Reset(string targetSystem, string targetTable)
    {
        switch (targetSystem)
        {
            case "postgres":
                ResetPostgres(targetTable);
                break;
            case "mongodb":
                ResetMongo(targetTable);
                break;
            default:
                throw new InvalidOperationException($"Unknown target system '{targetSystem}'.");
        }
    }

    private static void ResetPostgres(string targetTable)
    {
        using var connection = new NpgsqlConnection(PostgresConnectionString);
        connection.Open();

        using (var truncateCommand = connection.CreateCommand())
        {
            truncateCommand.CommandText = $"TRUNCATE TABLE \"{targetTable}\" RESTART IDENTITY";
            truncateCommand.ExecuteNonQuery();
        }
        var insertSql = targetTable switch
        {
            "students" => """
                INSERT INTO "students" (first_name, last_name, email, year) VALUES
                ('Petra', 'Perić', 'petra.peric@example.com', 1),
                ('Ivan', 'Ivanović', 'ivan.ivanovic@example.com', 2),
                ('Luka', 'Lukić', 'luka.lukic@example.com', 3)
                """,
            _ => null,
        };

        if (insertSql is null)
        {
            return;
        }
        using var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = insertSql;
        insertCommand.ExecuteNonQuery();
    }

    private static void ResetMongo(string targetCollection)
    {
        var database = new MongoClient(MongoConnectionString).GetDatabase(MongoDatabaseName);
        var collection = database.GetCollection<BsonDocument>(targetCollection);

        collection.DeleteMany(FilterDefinition<BsonDocument>.Empty);

        var defaults = targetCollection switch
        {
            "students" => new[]
            {
                new BsonDocument
                {
                    { "first_name", "Petra" }, { "last_name", "Perić" },
                    { "email", "petra.peric@example.com" }, { "year", 1 },
                },
                new BsonDocument
                {
                    { "first_name", "Ivan" }, { "last_name", "Ivanović" },
                    { "email", "ivan.ivanovic@example.com" }, { "year", 2 },
                },
                new BsonDocument
                {
                    { "first_name", "Luka" }, { "last_name", "Lukić" },
                    { "email", "luka.lukic@example.com" }, { "year", 3 },
                },
            },
            _ => Array.Empty<BsonDocument>(),
        };

        if (defaults.Length > 0)
        {
            collection.InsertMany(defaults);
        }
    }
}
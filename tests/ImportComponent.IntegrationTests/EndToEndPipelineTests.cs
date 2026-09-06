using System.Text;
using ImportComponent.BLL;
using ImportComponent.Core;
using ImportComponent.DAL.Mongo;
using ImportComponent.DAL.Postgres;
using ImportComponent.Readers;
using MongoDB.Bson;
using MongoDB.Driver;
using Npgsql;

namespace ImportComponent.IntegrationTests;

public class EndToEndPipelineTests
{
    private const string Csv = "name,score\nAna,10\nMarko,20\nIva,30\n";

    [Fact]
    public void FullPipeline_CsvToPostgres_AutoSuggestedMapping_Succeeds()
    {
        var tableName = "it_" + Guid.NewGuid().ToString("N")[..12];
        CreatePostgresTable(tableName);
        try
        {
            var introspector = new PostgresSchemaIntroSpector(TestConfig.PostgresConnectionString);
            var writerFactory = new PostgresWriterFactory(TestConfig.PostgresConnectionString);
            var orchestrator = new ImportOrchestrator(introspector, writerFactory);

            var result = RunPipeline(orchestrator, introspector, tableName);

            Assert.Equal(3, result.ValidCount);
            Assert.Empty(result.RejectedRecords);

            var summary = orchestrator.Confirm(result.RunId, tableName);
            Assert.Equal(3, summary.InsertedCount);
        }
        finally
        {
            DropPostgresTable(tableName);
        }
    }

    [Fact]
    public void FullPipeline_CsvToMongo_AutoSuggestedMapping_Succeeds()
    {
        var collectionName = "it_" + Guid.NewGuid().ToString("N")[..12];
        var database = new MongoClient(TestConfig.MongoConnectionString).GetDatabase(TestConfig.MongoDatabaseName);
        database.GetCollection<BsonDocument>(collectionName).InsertOne(new BsonDocument { { "name", "Seed" }, { "score", 0 } });

        try
        {
            var introspector = new MongoSchemaIntrospector(TestConfig.MongoConnectionString, TestConfig.MongoDatabaseName);
            var writerFactory = new MongoWriterFactory(TestConfig.MongoConnectionString, TestConfig.MongoDatabaseName);
            var orchestrator = new ImportOrchestrator(introspector, writerFactory);

            var result = RunPipeline(orchestrator, introspector, collectionName);

            Assert.Equal(3, result.ValidCount);
            Assert.Empty(result.RejectedRecords);

            var summary = orchestrator.Confirm(result.RunId, collectionName);
            Assert.Equal(3, summary.InsertedCount);
        }
        finally
        {
            database.DropCollection(collectionName);
        }
    }

    private static ImportRunResult RunPipeline(ImportOrchestrator orchestrator, ISchemaIntrospector introspector, string targetTable)
    {
        var reader = new CsvDataReader();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Csv));
        var schema = reader.InspectStructure(stream);
        stream.Position = 0;

        var targetColumns = introspector.ListColumns(targetTable);
        var mapping = FieldMappingSuggester.Suggest(schema.FieldNames, targetColumns);

        Assert.Equal("name", mapping["name"]);
        Assert.Equal("score", mapping["score"]);

        return orchestrator.RunToStaging(reader, stream, new ReaderOptions(), targetTable, mapping);
    }

    private static void CreatePostgresTable(string tableName)
    {
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
    }

    private static void DropPostgresTable(string tableName)
    {
        using var connection = new NpgsqlConnection(TestConfig.PostgresConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"DROP TABLE IF EXISTS \"{tableName}\"";
        command.ExecuteNonQuery();
    }
}
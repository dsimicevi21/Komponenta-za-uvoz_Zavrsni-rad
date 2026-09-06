using ImportComponent.BLL;
using ImportComponent.Core;
using ImportComponent.DAL.Mongo;
using ImportComponent.DAL.Postgres;
using ImportComponent.Readers;
using MongoDB.Bson;
using MongoDB.Driver;
using Npgsql;

namespace ImportComponent.IntegrationTests;

public class KnjigeDomainIntegrationTests
{
    private const string KnjigeCsv =
        "isbn,naziv,cijena,dostupna,zanr\n" +
        "ISBN-T100,Testna knjiga,12.50,true,Test\n" +
        "ISBN-T101,Testna zbirka,7.25,false,Test\n";

    [Fact]
    public void FullPipeline_KnjigeCsvToPostgres_CijenaIDostupnaLandWithCorrectTypes()
    {
        using var connection = new NpgsqlConnection(TestConfig.PostgresConnectionString);
        connection.Open();

        using (var cleanupCommand = connection.CreateCommand())
        {
            cleanupCommand.CommandText = "DELETE FROM knjige WHERE isbn IN ('ISBN-T100', 'ISBN-T101')";
            cleanupCommand.ExecuteNonQuery();
        }

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT cijena, dostupna FROM knjige WHERE isbn = 'ISBN-T100'";

        var introspector = new PostgresSchemaIntroSpector(TestConfig.PostgresConnectionString);
        var writerFactory = new PostgresWriterFactory(TestConfig.PostgresConnectionString);
        var orchestrator = new ImportOrchestrator(introspector, writerFactory);
        var reader = new CsvDataReader();

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(KnjigeCsv));
        var schema = reader.InspectStructure(stream);
        stream.Position = 0;

        var targetColumns = introspector.ListColumns("knjige");
        var mapping = FieldMappingSuggester.Suggest(schema.FieldNames, targetColumns);
        Assert.Equal(5, mapping.Count);

        var result = orchestrator.RunToStaging(reader, stream, new ReaderOptions(), "knjige", mapping);
        Assert.Equal(2, result.ValidCount);
        Assert.Empty(result.RejectedRecords);

        var summary = orchestrator.Confirm(result.RunId, "knjige");
        Assert.Equal(2, summary.InsertedCount);

        using var readerResult = command.ExecuteReader();
        Assert.True(readerResult.Read());
        Assert.Equal(12.50m, readerResult.GetDecimal(0));
        Assert.True(readerResult.GetBoolean(1));
    }

    [Fact]
    public void FullPipeline_KnjigeCsvToMongo_CijenaIDostupnaLandWithCorrectTypes()
    {
        var database = new MongoClient(TestConfig.MongoConnectionString).GetDatabase(TestConfig.MongoDatabaseName);

        database.GetCollection<BsonDocument>("knjige")
            .DeleteMany(Builders<BsonDocument>.Filter.In("isbn", new[] { "ISBN-T100", "ISBN-T101" }));

        var introspector = new MongoSchemaIntrospector(TestConfig.MongoConnectionString, TestConfig.MongoDatabaseName);
        var writerFactory = new MongoWriterFactory(TestConfig.MongoConnectionString, TestConfig.MongoDatabaseName);
        var orchestrator = new ImportOrchestrator(introspector, writerFactory);
        var reader = new CsvDataReader();

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(KnjigeCsv));
        var schema = reader.InspectStructure(stream);
        stream.Position = 0;

        var targetColumns = introspector.ListColumns("knjige");
        var mapping = FieldMappingSuggester.Suggest(schema.FieldNames, targetColumns);

        var result = orchestrator.RunToStaging(reader, stream, new ReaderOptions(), "knjige", mapping);
        Assert.Equal(2, result.ValidCount);
        Assert.Empty(result.RejectedRecords);

        var summary = orchestrator.Confirm(result.RunId, "knjige");
        Assert.Equal(2, summary.InsertedCount);

        var document = database.GetCollection<BsonDocument>("knjige")
            .Find(Builders<BsonDocument>.Filter.Eq("isbn", "ISBN-T100"))
            .FirstOrDefault();

        Assert.NotNull(document);
        Assert.Equal(BsonType.Boolean, document["dostupna"].BsonType);
        Assert.True(document["dostupna"].AsBoolean);
        Assert.True(document["cijena"].BsonType is BsonType.Decimal128 or BsonType.Double);
    }
}

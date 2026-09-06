using ImportComponent.Core;
using ImportComponent.DAL.Mongo;
using ImportComponent.DAL.Postgres;
using MongoDB.Bson;
using MongoDB.Driver;
using Npgsql;

namespace ImportComponent.IntegrationTests;
public class StagingCleanupIntegrationTests
{
    [Fact]
    public void Postgres_CleanupOrphanedStaging_RemovesOnlyStaleStagingTables()
    {
        var factory = new PostgresWriterFactory(TestConfig.PostgresConnectionString);
        var oldName = "staging_" + DateTimeOffset.UtcNow.AddHours(-2).ToUnixTimeSeconds() + "_" + Guid.NewGuid().ToString("N");
        var freshName = "staging_" + StagingNaming.NewRunId();

        using (var connection = new NpgsqlConnection(TestConfig.PostgresConnectionString))
        {
            connection.Open();
            CreateEmptyTable(connection, oldName);
            CreateEmptyTable(connection, freshName);
        }

        try
        {
            var cleaned = factory.CleanupOrphanedStaging(TimeSpan.FromMinutes(15));

            Assert.Equal(1, cleaned);
            Assert.False(TableExists(oldName));
            Assert.True(TableExists(freshName));
            Assert.True(TableExists("students"));
        }
        finally
        {
            DropTableIfExists(oldName);
            DropTableIfExists(freshName);
        }
    }

    [Fact]
    public void Mongo_CleanupOrphanedStaging_RemovesOnlyStaleStagingCollections()
    {
        var factory = new MongoWriterFactory(TestConfig.MongoConnectionString, TestConfig.MongoDatabaseName);
        var database = new MongoClient(TestConfig.MongoConnectionString).GetDatabase(TestConfig.MongoDatabaseName);

        var oldName = "staging_" + DateTimeOffset.UtcNow.AddHours(-2).ToUnixTimeSeconds() + "_" + Guid.NewGuid().ToString("N");
        var freshName = "staging_" + StagingNaming.NewRunId();

        database.CreateCollection(oldName);
        database.CreateCollection(freshName);

        try
        {
            var cleaned = factory.CleanupOrphanedStaging(TimeSpan.FromMinutes(15));

            Assert.Equal(1, cleaned);
            Assert.False(CollectionExists(database, oldName));
            Assert.True(CollectionExists(database, freshName));
            Assert.True(CollectionExists(database, "students"));
        }
        finally
        {
            database.DropCollection(oldName);
            database.DropCollection(freshName);
        }
    }

    private static void CreateEmptyTable(NpgsqlConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"CREATE TABLE \"{tableName}\" (id SERIAL PRIMARY KEY)";
        command.ExecuteNonQuery();
    }

    private static bool TableExists(string tableName)
    {
        using var connection = new NpgsqlConnection(TestConfig.PostgresConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT to_regclass(@name) IS NOT NULL";
        command.Parameters.AddWithValue("name", tableName);
        return (bool)command.ExecuteScalar()!;
    }

    private static void DropTableIfExists(string tableName)
    {
        using var connection = new NpgsqlConnection(TestConfig.PostgresConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"DROP TABLE IF EXISTS \"{tableName}\"";
        command.ExecuteNonQuery();
    }

    private static bool CollectionExists(IMongoDatabase database, string collectionName) =>
        database.ListCollectionNames(new ListCollectionNamesOptions
        {
            Filter = Builders<BsonDocument>.Filter.Eq("name", collectionName),
        }).Any();
}
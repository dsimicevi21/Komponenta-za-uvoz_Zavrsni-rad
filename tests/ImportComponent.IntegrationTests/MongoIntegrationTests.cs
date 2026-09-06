using ImportComponent.Core;
using ImportComponent.DAL.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;
using Record = ImportComponent.Core.Record;

namespace ImportComponent.IntegrationTests;

public class MongoIntegrationTests
{
    [Fact]
    public void ListTables_IncludesAFreshlyCreatedCollection()
    {
        var collectionName = CreateScratchCollection();
        try
        {
            var introspector = new MongoSchemaIntrospector(TestConfig.MongoConnectionString, TestConfig.MongoDatabaseName);

            var tables = introspector.ListTables();

            Assert.Contains(tables, t => t.Name == collectionName);
        }
        finally
        {
            DropCollection(collectionName);
        }
    }

    [Fact]
    public void ListColumns_InfersFieldsFromSampledDocument()
    {
        var collectionName = CreateScratchCollection();
        try
        {
            var introspector = new MongoSchemaIntrospector(TestConfig.MongoConnectionString, TestConfig.MongoDatabaseName);

            var columns = introspector.ListColumns(collectionName);

            var id = Assert.Single(columns, c => c.Name == "_id");
            Assert.False(id.IsNullable);
            Assert.True(id.HasDefault);

            var name = Assert.Single(columns, c => c.Name == "name");
            Assert.Equal("text", name.DataType);

            var score = Assert.Single(columns, c => c.Name == "score");
            Assert.Equal("integer", score.DataType);
        }
        finally
        {
            DropCollection(collectionName);
        }
    }

    [Fact]
    public void ListColumns_UnknownCollection_ThrowsTargetTableNotFoundException()
    {
        var introspector = new MongoSchemaIntrospector(TestConfig.MongoConnectionString, TestConfig.MongoDatabaseName);

        Assert.Throws<TargetTableNotFoundException>(
            () => introspector.ListColumns("does_not_exist_" + Guid.NewGuid().ToString("N")));
    }

    [Fact]
    public void WriteToStaging_ThenCommit_DocumentsLandInTargetCollection_AndStagingIsDropped()
    {
        var collectionName = CreateScratchCollection();
        try
        {
            var writer = new MongoDataWriter(TestConfig.MongoConnectionString, TestConfig.MongoDatabaseName, collectionName);
            var runId = Guid.NewGuid().ToString("N");

            var records = new List<Record>
            {
                new() { ["name"] = "Ana", ["score"] = 10 },
                new() { ["name"] = "Marko", ["score"] = 20 },
            };

            writer.WriteToStaging(runId, records);
            var summary = writer.CommitStaging(runId);

            Assert.Equal(2, summary.InsertedCount);
            Assert.Equal(3, CountDocuments(collectionName));
            Assert.False(CollectionExists(StagingCollectionName(runId)));
        }
        finally
        {
            DropCollection(collectionName);
        }
    }

    [Fact]
    public void WriteToStaging_ThenDiscard_TargetCollectionUnchanged_AndStagingIsDropped()
    {
        var collectionName = CreateScratchCollection();
        try
        {
            var writer = new MongoDataWriter(TestConfig.MongoConnectionString, TestConfig.MongoDatabaseName, collectionName);
            var runId = Guid.NewGuid().ToString("N");

            writer.WriteToStaging(runId, new List<Record> { new() { ["name"] = "Ana", ["score"] = 10 } });
            writer.DiscardStaging(runId);

            Assert.Equal(1, CountDocuments(collectionName));
            Assert.False(CollectionExists(StagingCollectionName(runId)));
        }
        finally
        {
            DropCollection(collectionName);
        }
    }

    [Fact]
    public void WriteToStaging_TargetCollectionDoesNotExist_ThrowsTargetTableNotFoundException()
    {
        var writer = new MongoDataWriter(
            TestConfig.MongoConnectionString, TestConfig.MongoDatabaseName, "does_not_exist_" + Guid.NewGuid().ToString("N"));

        Assert.Throws<TargetTableNotFoundException>(
            () => writer.WriteToStaging(Guid.NewGuid().ToString("N"), new List<Record> { new() { ["name"] = "Ana" } }));
    }

    private static IMongoDatabase Database() =>
        new MongoClient(TestConfig.MongoConnectionString).GetDatabase(TestConfig.MongoDatabaseName);

    private static string StagingCollectionName(string runId) =>
        "staging_" + new string(runId.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());

    private static string CreateScratchCollection()
    {
        var collectionName = "it_" + Guid.NewGuid().ToString("N")[..12];
        var collection = Database().GetCollection<BsonDocument>(collectionName);
        collection.InsertOne(new BsonDocument { { "name", "Seed" }, { "score", 0 } });
        return collectionName;
    }

    private static void DropCollection(string collectionName) => Database().DropCollection(collectionName);

    private static bool CollectionExists(string collectionName) =>
        Database().ListCollectionNames(new ListCollectionNamesOptions
        {
            Filter = Builders<BsonDocument>.Filter.Eq("name", collectionName),
        }).Any();

    private static long CountDocuments(string collectionName) =>
        Database().GetCollection<BsonDocument>(collectionName).CountDocuments(FilterDefinition<BsonDocument>.Empty);
}
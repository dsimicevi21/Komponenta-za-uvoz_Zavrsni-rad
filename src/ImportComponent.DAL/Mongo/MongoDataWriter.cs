using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImportComponent.Core;
using MongoDB.Driver;
using MongoDB.Bson;

namespace ImportComponent.DAL.Mongo;

public sealed class MongoDataWriter : IDataWriter
{
    private readonly IMongoDatabase _database;
    private readonly string _targetCollection;

    public MongoDataWriter(string connectionString, string databaseName, string targetCollection)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
        _targetCollection = targetCollection;
    }

    public void WriteToStaging(string runId, IEnumerable<Record> records)
    {
        if (!CollectionExists(_targetCollection))
        {
            throw new TargetTableNotFoundException(_targetCollection);
        }

        var documents = records.Select(RecordToBsonDocument).ToList();
        if (documents.Count == 0)
        {
            return;
        }

        _database.GetCollection<BsonDocument>(StagingCollectionName(runId)).InsertMany(documents);
    }

    public ImportSummary CommitStaging(string runId)
    {
        var stagingName = StagingCollectionName(runId);
        var staging = _database.GetCollection<BsonDocument>(stagingName);
        var target = _database.GetCollection<BsonDocument>(_targetCollection);

        var documents = staging.Find(FilterDefinition<BsonDocument>.Empty).ToList();
        if (documents.Count > 0)
        {
            target.InsertMany(documents);
        }
        _database.DropCollection(stagingName);

        return new ImportSummary
        {
            RunId = runId,
            InsertedCount = documents.Count,
        };
    }

    public void DiscardStaging(string runId) => _database.DropCollection(StagingCollectionName(runId));

    private bool CollectionExists(string name) =>
        _database.ListCollectionNames(new ListCollectionNamesOptions
        {
            Filter = Builders<BsonDocument>.Filter.Eq("name", name),
        }).Any();

    private static string StagingCollectionName(string runId) => "staging_" + new string(runId.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());

    private static BsonDocument RecordToBsonDocument(Record record)
    {
        var document = new BsonDocument();
        foreach (var (key, value) in record.Fields)
        {
            document[key] = ToBsonValue(value);
        }
        return document;
    }

    private static BsonValue ToBsonValue(object value) => value switch
    {
        null => BsonNull.Value,
        Record nested => RecordToBsonDocument(nested),
        IEnumerable<object> list => new BsonArray(list.Select(ToBsonValue)),
        _ => BsonValue.Create(value),
    };
}

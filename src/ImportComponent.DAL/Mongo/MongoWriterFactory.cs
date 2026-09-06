using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImportComponent.Core;
using MongoDB.Driver;

namespace ImportComponent.DAL.Mongo;

public sealed class MongoWriterFactory : IDataWriterFactory
{
    private readonly string _connectionString;
    private readonly string _databaseName;

    public MongoWriterFactory(string connectionString, string databaseName)
    {
        _connectionString = connectionString;
        _databaseName = databaseName;
    }

    public string TargetSystemId => "mongodb";

    public IDataWriter CreateWriter(string targetTable) => 
        new MongoDataWriter(_connectionString, _databaseName, targetTable);

    public int CleanupOrphanedStaging(TimeSpan olderThan)
    {
        var database = new MongoClient(_connectionString).GetDatabase(_databaseName);
        var collectionNames = database.ListCollectionNames().ToList();

        var cutoff = DateTimeOffset.UtcNow - olderThan;
        var droppedCount = 0;

        foreach (var name in collectionNames)
        {
            if (!StagingNaming.TryParseCreatedAt(name, out var createdAt) || createdAt >= cutoff)
            {
                continue;
            }
            database.DropCollection(name);
            droppedCount++;
        }
        return droppedCount;
    }

}

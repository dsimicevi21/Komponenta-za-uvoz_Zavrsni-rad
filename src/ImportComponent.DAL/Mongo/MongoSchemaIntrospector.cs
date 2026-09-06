using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImportComponent.Core;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ImportComponent.DAL.Mongo;

public sealed class MongoSchemaIntrospector   :  ISchemaIntrospector
{
    private const int SampleSize = 50;

    private readonly IMongoDatabase _database;

    public MongoSchemaIntrospector(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }
    public IReadOnlyList<TableInfo> ListTables() =>
        _database.ListCollectionNames().ToList().Select(name => new TableInfo { Name = name }).ToList();
    
    public IReadOnlyList<ColumnInfo> ListColumns(string tableName)
    {
        if (!CollectionExists(tableName))
        {
            throw new TargetTableNotFoundException(tableName);
        }
        var collection = _database.GetCollection<BsonDocument>(tableName);
        var sample = collection.Find(FilterDefinition<BsonDocument>.Empty).Limit(SampleSize).ToList();
        var fieldTypes = new Dictionary<string, string>();
        foreach (var document in sample)
        {
            foreach (var element in document.Elements)
            {
                fieldTypes[element.Name] = MapBsonType(element.Value);
            }
        }

        return fieldTypes.Select(field => new ColumnInfo
        {
            Name = field.Key,
            DataType = field.Value,
            IsNullable = field.Key != "_id", 
            HasDefault = field.Key == "_id",
        }).ToList();
    }
    
    private bool CollectionExists(string name) =>
    _database.ListCollectionNames(new ListCollectionNamesOptions
    {
        Filter = Builders<BsonDocument>.Filter.Eq("name", name),
    }).Any();

    private static string MapBsonType(BsonValue value) => value.BsonType switch
    {
        BsonType.String => "text",
        BsonType.Int32 => "integer",
        BsonType.Int64 => "bigint",
        BsonType.Double => "double precision",
        BsonType.Decimal128 => "numeric",
        BsonType.Boolean => "boolean",
        BsonType.DateTime => "timestamp without time zone",
        _ => "text",
    };
}

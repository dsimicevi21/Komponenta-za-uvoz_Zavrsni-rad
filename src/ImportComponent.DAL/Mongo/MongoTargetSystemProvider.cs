using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImportComponent.Core;

namespace ImportComponent.DAL.Mongo;

public sealed class MongoTargetSystemProvider : ITargetSystemProvider
{
    private readonly string _connectionString;
    private readonly string _databaseName;

    public MongoTargetSystemProvider(string connectionString, string databaseName)
    {
        _connectionString = connectionString;
        _databaseName = databaseName;
    }

    public string SystemId => "mongodb";

    public ISchemaIntrospector CreateIntrospector() => new MongoSchemaIntrospector(_connectionString, _databaseName);

    public IDataWriterFactory CreateWriterFactory() => new MongoWriterFactory(_connectionString, _databaseName);
}

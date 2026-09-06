using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImportComponent.Core;

namespace ImportComponent.DAL.Postgres;

public sealed class PostgresTargetSystemProvider
{
    private readonly string _connectionString;

    public PostgresTargetSystemProvider(string connectionString)
    {
        _connectionString = connectionString;
    }

    public string SystemId => "postgres";

    public ISchemaIntrospector CreateIntrospector() => new PostgresSchemaIntroSpector(_connectionString);

    public IDataWriterFactory CreateWriterFactory() => new PostgresWriterFactory(_connectionString);
}

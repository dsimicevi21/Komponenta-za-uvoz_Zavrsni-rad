using ImportComponent.Api;
using ImportComponent.Core;
using ImportComponent.DAL.Mongo;
using ImportComponent.DAL.Postgres;
using ImportComponent.Readers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IDataReader, CsvDataReader>();
builder.Services.AddSingleton<IDataReader, JsonDataReader>();
builder.Services.AddSingleton<IDataReader, XmlDataReader>();
builder.Services.AddSingleton<IDataReaderRegistry, DataReaderRegistry>();

var postgresConnectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? "Host=localhost; POrt=5432;Username=postgres;Password=postgres;Database=Import_demo";
var mongoConnectionString = builder.Configuration.GetConnectionString("Mongo")
    ?? "mongodb://localhost:27017";
var mongoDatabaseName = builder.Configuration["MongoDatabaseName"] ?? "Import_demo";

builder.Services.AddSingleton<ITargetSystemProvider>(
    _ => new PostgresTargetSystemProvider(postgresConnectionString));
builder.Services.AddSingleton<ITargetSystemProvider>(
    _ => new MongoTargetSystemProvider(mongoConnectionString, mongoDatabaseName));

builder.Services.AddSingleton<PendingRunStore>();

var app = builder.Build();

var cleanupThreshold = TimeSpan.FromMinutes(15);
foreach (var provider in app.Services.GetServices<ITargetSystemProvider>())
{
    try
    {
        var cleaned = provider.CreateWriterFactory().CleanupOrphanedStaging(cleanupThreshold);
        if (cleaned > 0)
        {
            Console.WriteLine($"[startup cleanup] {provider.SystemId}: removed {cleaned} orphaned staging artifact(s).");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[startup cleanup] {provider.SystemId}: cleanup skipped - {ex.Message}");
    }
}

app.UseHttpsRedirection();

app.MapImportEndpoints();

app.Run();
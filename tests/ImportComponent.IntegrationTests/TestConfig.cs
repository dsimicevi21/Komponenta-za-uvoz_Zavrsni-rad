namespace ImportComponent.IntegrationTests;

internal static class TestConfig
{
    public const string PostgresConnectionString =
        "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=import_demo";

    public const string MongoConnectionString = "mongodb://localhost:27017";
    public const string MongoDatabaseName = "import_demo";
}
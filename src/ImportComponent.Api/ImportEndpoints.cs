using ImportComponent.Api;
using ImportComponent.Api.Contracts;
using ImportComponent.BLL;
using ImportComponent.Core;
using System.Text.Json;

namespace ImportComponent.Api;

public static class ImportEndpoints
{
        public static void MapImportEndpoints(this WebApplication app)
        {
            app.MapGet("/api/targets", (IEnumerable<ITargetSystemProvider> providers) =>
                providers.Select(p => new TargetSystemDto(p.SystemId)));

            app.MapGet("/api/targets/{system}/tables", (string system, IEnumerable<ITargetSystemProvider> providers) =>
            {
                var provider = ResolveProvider(providers, system);
                if (provider is null)
                {
                    return Results.NotFound($"Unknown target system '{system}'.");
                }

                var tables = provider.CreateIntrospector().ListTables()
                    .Select(t => new TableDto(t.Name));
                return Results.Ok(tables);
            });

            app.MapGet("/api/targets/{system}/tables/{tableName}/columns", (
                string system, string tableName, IEnumerable<ITargetSystemProvider> providers) =>
            {
                var provider = ResolveProvider(providers, system);
                if (provider is null)
                {
                    return Results.NotFound($"Unknown target system '{system}'.");
                }

                try
                {
                    var columns = provider.CreateIntrospector().ListColumns(tableName)
                        .Select(c => new ColumnDto(c.Name, c.DataType, c.IsNullable, c.HasDefault));
                    return Results.Ok(columns);
                }
                catch (TargetTableNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            });

            app.MapPost("/api/imports/inspect", async (HttpRequest request, IDataReaderRegistry readerRegistry) =>
            {
                if (!request.HasFormContentType)
                {
                    return Results.BadRequest("Expected multipart/form-data.");
                }

                var form = await request.ReadFormAsync();
                var file = form.Files.GetFile("file");
                if (file is null)
                {
                    return Results.BadRequest("Missing 'file' part.");
                }

                var entityName = form["entityName"].ToString();
                var sample = await ReadSampleAsync(file);
                var reader = readerRegistry.FindReader(file.FileName, sample);
                if (reader is null)
                {
                    return Results.BadRequest($"No reader recognizes '{file.FileName}'.");
                }

                await using var stream = file.OpenReadStream();
                var schema = reader.InspectStructure(stream, string.IsNullOrWhiteSpace(entityName) ? null : entityName);

                return Results.Ok(new InspectResultDto(reader.FormatId, schema.FieldNames, schema.EntityNames));
            });

            app.MapPost("/api/imports/suggest-mapping", (SuggestMappingRequest body, IEnumerable<ITargetSystemProvider> providers) =>
            {
                var provider = ResolveProvider(providers, body.TargetSystem);
                if (provider is null)
                {
                    return Results.NotFound($"Unknown target system '{body.TargetSystem}'.");
                }

                try
                {
                    var columns = provider.CreateIntrospector().ListColumns(body.TargetTable);
                    var suggestions = FieldMappingSuggester.Suggest(body.SourceFieldNames, columns);
                    return Results.Ok(suggestions);
                }
                catch (TargetTableNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            });

            app.MapPost("/api/imports/run", async (
                HttpRequest request,
                IEnumerable<ITargetSystemProvider> providers,
                IDataReaderRegistry readerRegistry,
                PendingRunStore runStore) =>
            {
                if (!request.HasFormContentType)
                {
                    return Results.BadRequest("Expected multipart/form-data.");
                }

                var form = await request.ReadFormAsync();
                var file = form.Files.GetFile("file");
                if (file is null)
                {
                    return Results.BadRequest("Missing 'file' part.");
                }

                var targetSystem = form["targetSystem"].ToString();
                var targetTable = form["targetTable"].ToString();
                var entityName = form["entityName"].ToString();
                var fieldMappingJson = form["fieldMapping"].ToString();

                if (string.IsNullOrWhiteSpace(targetSystem) || string.IsNullOrWhiteSpace(targetTable)
                    || string.IsNullOrWhiteSpace(fieldMappingJson))
                {
                    return Results.BadRequest("targetSystem, targetTable and fieldMapping are required form fields.");
                }

                var provider = ResolveProvider(providers, targetSystem);
                if (provider is null)
                {
                    return Results.NotFound($"Unknown target system '{targetSystem}'.");
                }

                var fieldMapping = JsonSerializer.Deserialize<Dictionary<string, string>>(fieldMappingJson)
                    ?? new Dictionary<string, string>();

                var sample = await ReadSampleAsync(file);
                var reader = readerRegistry.FindReader(file.FileName, sample);
                if (reader is null)
                {
                    return Results.BadRequest($"No reader recognizes '{file.FileName}'.");
                }

                var orchestrator = new ImportOrchestrator(provider.CreateIntrospector(), provider.CreateWriterFactory());
                var options = new ReaderOptions { EntityName = string.IsNullOrWhiteSpace(entityName) ? null : entityName };

                ImportRunResult runResult;
                await using (var stream = file.OpenReadStream())
                {
                    try
                    {
                        runResult = orchestrator.RunToStaging(reader, stream, options, targetTable, fieldMapping);
                    }
                    catch (TargetTableNotFoundException ex)
                    {
                        return Results.NotFound(ex.Message);
                    }
                }

                runStore.Add(new PendingRun(runResult.RunId, targetSystem, targetTable));

                return Results.Ok(new RunImportResponse(
                    runResult.RunId,
                    runResult.TargetTable,
                    runResult.ValidCount,
                    runResult.RejectedRecords.Select(r => new RejectedRecordDto(r.Reason)).ToList(),
                    runResult.ValidRecords.Select(r => (IReadOnlyDictionary<string, object?>)r.Fields).ToList()));
            });

            app.MapPost("/api/imports/{runId}/confirm", (
                string runId, IEnumerable<ITargetSystemProvider> providers, PendingRunStore runStore) =>
            {
                var pending = runStore.Get(runId);
                if (pending is null)
                {
                    return Results.NotFound($"No pending run '{runId}'.");
                }

                var provider = ResolveProvider(providers, pending.TargetSystem)!;
                var orchestrator = new ImportOrchestrator(provider.CreateIntrospector(), provider.CreateWriterFactory());
                var summary = orchestrator.Confirm(runId, pending.TargetTable);
                runStore.Remove(runId);

                return Results.Ok(new ImportSummaryDto(summary.RunId, summary.InsertedCount));
            });

            app.MapPost("/api/imports/{runId}/discard", (
                string runId, IEnumerable<ITargetSystemProvider> providers, PendingRunStore runStore) =>
            {
                var pending = runStore.Get(runId);
                if (pending is null)
                {
                    return Results.NotFound($"No pending run '{runId}'.");
                }

                var provider = ResolveProvider(providers, pending.TargetSystem)!;
                var orchestrator = new ImportOrchestrator(provider.CreateIntrospector(), provider.CreateWriterFactory());
                orchestrator.Discard(runId, pending.TargetTable);
                runStore.Remove(runId);

                return Results.NoContent();
            });
        }

        private static ITargetSystemProvider? ResolveProvider(IEnumerable<ITargetSystemProvider> providers, string systemId) =>
            providers.FirstOrDefault(p => string.Equals(p.SystemId, systemId, StringComparison.OrdinalIgnoreCase));

        private static async Task<byte[]> ReadSampleAsync(IFormFile file)
        {
            const int sampleSize = 4096;
            await using var stream = file.OpenReadStream();
            var buffer = new byte[Math.Min(sampleSize, (int)file.Length)];
            var read = await stream.ReadAsync(buffer);
            return buffer[..read];
        }
}



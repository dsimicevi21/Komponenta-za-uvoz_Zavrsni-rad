using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImportComponent.Core;

namespace ImportComponent.BLL;

public sealed class ImportOrchestrator
{
    private readonly ISchemaIntrospector _schemaIntrospector;
    private readonly IDataWriterFactory _writerFactory;
    private readonly RecordValidator _validator = new();

    public ImportOrchestrator(ISchemaIntrospector schemaIntrospector,IDataWriterFactory writerFactory)
    {
        _schemaIntrospector = schemaIntrospector;
        _writerFactory = writerFactory;
    }

    public ImportRunResult RunToStaging(
        IDataReader reader,
        Stream source,
        ReaderOptions options,
        string targetTable,
        IReadOnlyDictionary<string, string> sourceToTargetField)
    {
        var targetColumns = _schemaIntrospector.ListColumns(targetTable);
        var writer = _writerFactory.CreateWriter(targetTable);

        var validRecords = new List<Record>();
        var rejectedRecords = new List<RejectedRecord>();

        foreach (var sourceRecord in reader.ReadAll(source, options))
        {
               var mapped = FIeldMapper.Map(sourceRecord, sourceToTargetField);
               var validation = _validator.Validate(mapped, targetColumns);

            if (!validation.IsValid)
            {
                 rejectedRecords.Add(new RejectedRecord { Record = sourceRecord, Reason = validation.RejectionReason! });
                 continue;
            }
            validRecords.Add(validation.Record!);
        }
        var runId = StagingNaming.NewRunId();
        writer.WriteToStaging(runId, validRecords);

        return new ImportRunResult
        {
            RunId = runId,
            TargetTable = targetTable,
            ValidCount = validRecords.Count,
            RejectedRecords = rejectedRecords,
            ValidRecords = validRecords
        };
    }

    public ImportSummary Confirm(string runId, string targetTable)
    {
        return _writerFactory.CreateWriter(targetTable).CommitStaging(runId);
    }

    public void Discard(string runId, string targetTable)
    {
        _writerFactory.CreateWriter(targetTable).DiscardStaging(runId);
    }
}

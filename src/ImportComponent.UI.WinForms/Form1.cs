using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Accessibility;

namespace ImportComponent.UI.WinForms;

public partial class Form1 : Form
{
    private const string NoMappingPlaceholder = "(nije mapirano)";

    private ApiClient? _apiClient;
    private string? _selectedFilePath;
    private List<string> _sourceFieldNames = new();
    private List<ColumnDto> _targetColumns = new();

    private string? _pendingRunId;

    public Form1()
    {
        InitializeComponent();
    }

    private async void LoadTargetsButton_Click(object? sender, EventArgs e)
    {
        _apiClient?.Dispose();
        _apiClient = new ApiClient(_apiUrlTextBox.Text.Trim());

        try
        {
            SetStatus("Loading target systems...", isError: false);
            var systems = await _apiClient.GetTargetSystemsAsync();

            _targetSystemCombo.Items.Clear();
            _resetTargetSystemCombo.Items.Clear();
            foreach (var system in systems)
            {
                _targetSystemCombo.Items.Add(system.SystemId);
                _resetTargetSystemCombo.Items.Add(system.SystemId);
            }

            _targetTableCombo.Items.Clear();
            ClearMappingGrid();

            if (_targetSystemCombo.Items.Count > 0)
            {
                _targetSystemCombo.SelectedIndex = 0;
            }

            if (_resetTargetSystemCombo.Items.Count > 0)
            {
                _resetTargetSystemCombo.SelectedIndex = 0;
            }

            SetStatus($"Loaded {systems.Count} target system(s).", isError: false);
        }
        catch (Exception ex)
        {
            SetStatus($"Failed to load target systems: {ex.Message}", isError: true);
        }
    }

    private async void TargetSystemCombo_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_apiClient is null || _targetSystemCombo.SelectedItem is not string system)
        {
            return;
        }

        try
        {
            SetStatus($"Loading tables for '{system}'...", isError: false);
            var tables = await _apiClient.GetTablesAsync(system);

            _targetTableCombo.Items.Clear();
            foreach (var table in tables)
            {
                _targetTableCombo.Items.Add(table.Name);
            }

            ClearMappingGrid();

            if (_targetTableCombo.Items.Count > 0)
            {
                _targetTableCombo.SelectedIndex = 0;
            }

            SetStatus($"Loaded {tables.Count} table(s) for '{system}'.", isError: false);
        }
        catch (Exception ex)
        {
            SetStatus($"Failed to load tables: {ex.Message}", isError: true);
        }
    }

    private async void ResetTargetSystemCombo_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_apiClient is null || _resetTargetSystemCombo.SelectedItem is not string system)
        {
            return;
        }

        try
        {
            var tables = await _apiClient.GetTablesAsync(system);

            _resetTargetTableCombo.Items.Clear();
            foreach (var table in tables)
            {
                _resetTargetTableCombo.Items.Add(table.Name);
            }

            if (_resetTargetTableCombo.Items.Count > 0)
            {
                _resetTargetTableCombo.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Failed to load tables for reset: {ex.Message}", isError: true);
        }
    }

    private void ResetButton_Click(object? sender, EventArgs e)
    {
        if (_resetTargetSystemCombo.SelectedItem is not string system
            || _resetTargetTableCombo.SelectedItem is not string table)
        {
            SetStatus("Odaberi ciljni sustav i tablicu za reset.", isError: true);
            return;
        }

        var confirmed = MessageBox.Show(
            this,
            $"Ovo će obrisati SVE retke u '{table}' ({system}) i vratiti nekoliko demo zapisa.\n\nNastaviti?",
            "Potvrda reseta",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirmed != DialogResult.Yes)
        {
            return;
        }

        try
        {
            DevResetService.Reset(system, table);
            SetStatus($"'{table}' ({system}) resetiran na demo zadane vrijednosti.", isError: false);
        }
        catch (Exception ex)
        {
            SetStatus($"Reset nije uspio: {ex.Message}", isError: true);
        }
    }

    private async void TargetTableCombo_SelectedIndexChanged(object? sender, EventArgs e)
    {
        await RefreshMappingGridAsync();
    }

    private async void SelectFileButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Podržane datoteke (*.csv;*.json;*.xml)|*.csv;*.json;*.xml|Sve datoteke (*.*)|*.*",
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        if (_apiClient is null)
        {
            SetStatus("Prvo klikni 'Load targets' da se spojiš na API.", isError: true);
            return;
        }

        _selectedFilePath = dialog.FileName;
        _selectedFileLabel.Text = Path.GetFileName(_selectedFilePath);
        ClearStagedRecordsGrid();
        ClearPendingRun();
        _resultTextBox.Clear();

        try
        {
            SetStatus("Inspecting file...", isError: false);
            var inspectResult = await _apiClient.InspectAsync(_selectedFilePath);

            _sourceFieldNames = inspectResult.FieldNames;

            _entityCombo.Items.Clear();
            foreach (var entityName in inspectResult.EntityNames)
            {
                _entityCombo.Items.Add(entityName);
            }

            if (_entityCombo.Items.Count > 0)
            {
                _entityCombo.SelectedIndex = 0;
            }

            SetStatus(
                $"Detected format '{inspectResult.FormatId}', {inspectResult.FieldNames.Count} field(s), " +
                $"{inspectResult.EntityNames.Count} entity(ies).",
                isError: false);

            await RefreshMappingGridAsync();
        }
        catch (Exception ex)
        {
            SetStatus($"Failed to inspect file: {ex.Message}", isError: true);
        }
    }

    private void EntityCombo_SelectedIndexChanged(object? sender, EventArgs e)
    {
    }

    private async Task RefreshMappingGridAsync()
    {
        if (_apiClient is null || _targetTableCombo.SelectedItem is not string targetTable
            || _targetSystemCombo.SelectedItem is not string targetSystem || _sourceFieldNames.Count == 0)
        {
            return;
        }

        try
        {
            SetStatus("Loading target columns and suggested mapping...", isError: false);

            _targetColumns = await _apiClient.GetColumnsAsync(targetSystem, targetTable);
            var suggestions = await _apiClient.SuggestMappingAsync(_sourceFieldNames, targetSystem, targetTable);

            _targetColumnColumn.Items.Clear();
            _targetColumnColumn.Items.Add(NoMappingPlaceholder);
            foreach (var column in _targetColumns)
            {
                _targetColumnColumn.Items.Add(column.Name);
            }

            _mappingGrid.Rows.Clear();
            foreach (var sourceField in _sourceFieldNames)
            {
                var target = suggestions.TryGetValue(sourceField, out var suggested) ? suggested : NoMappingPlaceholder;
                _mappingGrid.Rows.Add(sourceField, target);
            }

            SetStatus("Mapping ready - adjust it if needed, then run.", isError: false);
        }
        catch (Exception ex)
        {
            SetStatus($"Failed to load mapping: {ex.Message}", isError: true);
        }
    }

    private void ClearMappingGrid()
    {
        _mappingGrid.Rows.Clear();
        _targetColumnColumn.Items.Clear();
        _targetColumns = new List<ColumnDto>();
        ClearStagedRecordsGrid();
    }

    private void ClearStagedRecordsGrid()
    {
        _stagedRecordsGrid.Columns.Clear();
        _stagedRecordsGrid.Rows.Clear();
    }
    private void PopulateStagedRecordsGrid(List<Dictionary<string, JsonElement>> records)
    {
        ClearStagedRecordsGrid();

        if (records.Count == 0)
        {
            return;
        }

        var columnNames = records[0].Keys.ToList();
        foreach (var columnName in columnNames)
        {
            _stagedRecordsGrid.Columns.Add(columnName, columnName);
        }

        foreach (var record in records)
        {
            var rowValues = columnNames
                .Select(name => record.TryGetValue(name, out var value) ? FormatJsonElement(value) : string.Empty)
                .ToArray();
            _stagedRecordsGrid.Rows.Add(rowValues);
        }
    }

    private static string FormatJsonElement(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString() ?? string.Empty,
        JsonValueKind.Number => element.GetRawText(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Null => string.Empty,
        _ => element.GetRawText(),
    };
    private async void RunButton_Click(object? sender, EventArgs e)
    {
        if (_apiClient is null || _selectedFilePath is null)
        {
            SetStatus("Select a file first.", isError: true);
            return;
        }

        if (_targetSystemCombo.SelectedItem is not string targetSystem
            || _targetTableCombo.SelectedItem is not string targetTable)
        {
            SetStatus("Select a target system and table first.", isError: true);
            return;
        }

        var fieldMapping = new Dictionary<string, string>();
        foreach (DataGridViewRow row in _mappingGrid.Rows)
        {
            var sourceField = row.Cells[_sourceFieldColumn.Name].Value as string;
            var targetColumn = row.Cells[_targetColumnColumn.Name].Value as string;

            if (string.IsNullOrEmpty(sourceField) || string.IsNullOrEmpty(targetColumn)
                || targetColumn == NoMappingPlaceholder)
            {
                continue;
            }

            fieldMapping[sourceField] = targetColumn;
        }

        var entityName = _entityCombo.SelectedItem as string;

        try
        {
            SetStatus("Running import to staging...", isError: false);
            _runButton.Enabled = false;

            var result = await _apiClient.RunImportAsync(
                _selectedFilePath, targetSystem, targetTable, entityName, fieldMapping);

            _pendingRunId = result.RunId;

            var lines = new List<string>
            {
                $"Run ID: {result.RunId}",
                $"Target table: {result.TargetTable}",
                $"Valid records staged: {result.ValidCount}",
                $"Rejected records: {result.RejectedRecords.Count}",
            };
            lines.AddRange(result.RejectedRecords.Select(r => $"  REJECTED: {r.Reason}"));
            _resultTextBox.Text = string.Join(Environment.NewLine, lines);

            PopulateStagedRecordsGrid(result.ValidRecords);

            _confirmButton.Enabled = true;
            _discardButton.Enabled = true;

            SetStatus(
                $"Staged {result.ValidCount} record(s) - compare the grid below with pgAdmin/Compass, then Confirm or Discard.",
                isError: false);
        }
        catch (Exception ex)
        {
            SetStatus($"Run failed: {ex.Message}", isError: true);
        }
        finally
        {
            _runButton.Enabled = true;
        }
    }

    private async void ConfirmButton_Click(object? sender, EventArgs e)
    {
        if (_apiClient is null || _pendingRunId is null)
        {
            return;
        }

        try
        {
            var summary = await _apiClient.ConfirmAsync(_pendingRunId);
            _resultTextBox.Text += Environment.NewLine + Environment.NewLine +
                $"CONFIRMED: {summary.InsertedCount} record(s) committed to production.";
            SetStatus("Committed.", isError: false);
            ClearPendingRun();
        }
        catch (Exception ex)
        {
            SetStatus($"Confirm failed: {ex.Message}", isError: true);
        }
    }

    private async void DiscardButton_Click(object? sender, EventArgs e)
    {
        if (_apiClient is null || _pendingRunId is null)
        {
            return;
        }

        try
        {
            await _apiClient.DiscardAsync(_pendingRunId);
            _resultTextBox.Text += Environment.NewLine + Environment.NewLine + "DISCARDED: staging dropped, nothing written.";
            SetStatus("Discarded.", isError: false);
            ClearPendingRun();
        }
        catch (Exception ex)
        {
            SetStatus($"Discard failed: {ex.Message}", isError: true);
        }
    }

    private void ClearPendingRun()
    {
        _pendingRunId = null;
        _confirmButton.Enabled = false;
        _discardButton.Enabled = false;
    }

    private void SetStatus(string message, bool isError)
    {
        _statusLabel.Text = message;
        _statusLabel.ForeColor = isError ? Color.DarkRed : Color.DarkGreen;
    }
}
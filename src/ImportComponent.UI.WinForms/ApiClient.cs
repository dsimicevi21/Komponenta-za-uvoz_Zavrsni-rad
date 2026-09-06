using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text.Json;

namespace ImportComponent.UI.WinForms;

public sealed class ApiClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _http;

    public ApiClient(string baseUrl)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<List<TargetSystemDto>> GetTargetSystemsAsync() =>
        await _http.GetFromJsonAsync<List<TargetSystemDto>>("/api/targets", JsonOptions) ?? new();

    public async Task<List<TableDto>> GetTablesAsync(string system) =>
        await _http.GetFromJsonAsync<List<TableDto>>($"/api/targets/{system}/tables", JsonOptions) ?? new();

    public async Task<List<ColumnDto>> GetColumnsAsync(string system, string table)
    {
        using var response = await _http.GetAsync($"/api/targets/{system}/tables/{table}/columns");
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<List<ColumnDto>>(JsonOptions) ?? new();
    }

    public async Task<InspectResultDto> InspectAsync(string filePath)
    {
        using var content = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(filePath);
        content.Add(new StreamContent(fileStream), "file", Path.GetFileName(filePath));

        using var response = await _http.PostAsync("/api/imports/inspect", content);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<InspectResultDto>(JsonOptions)
            ?? throw new InvalidOperationException("Empty response from /api/imports/inspect.");
    }

    public async Task<Dictionary<string, string>> SuggestMappingAsync(
        IReadOnlyList<string> sourceFieldNames, string targetSystem, string targetTable)
    {
        using var response = await _http.PostAsJsonAsync(
            "/api/imports/suggest-mapping",
            new { sourceFieldNames, targetSystem, targetTable });
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(JsonOptions) ?? new();
    }

    public async Task<RunImportResponse> RunImportAsync(
        string filePath,
        string targetSystem,
        string targetTable,
        string? entityName,
        IReadOnlyDictionary<string, string> fieldMapping)
    {
        using var content = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(filePath);
        content.Add(new StreamContent(fileStream), "file", Path.GetFileName(filePath));
        content.Add(new StringContent(targetSystem), "targetSystem");
        content.Add(new StringContent(targetTable), "targetTable");
        if (!string.IsNullOrEmpty(entityName))
        {
            content.Add(new StringContent(entityName), "entityName");
        }

        content.Add(new StringContent(JsonSerializer.Serialize(fieldMapping)), "fieldMapping");

        using var response = await _http.PostAsync("/api/imports/run", content);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<RunImportResponse>(JsonOptions)
            ?? throw new InvalidOperationException("Empty response from /api/imports/run.");
    }

    public async Task<ImportSummaryDto> ConfirmAsync(string runId)
    {
        using var response = await _http.PostAsync($"/api/imports/{runId}/confirm", content: null);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<ImportSummaryDto>(JsonOptions)
            ?? throw new InvalidOperationException("Empty response from confirm.");
    }

    public async Task DiscardAsync(string runId)
    {
        using var response = await _http.PostAsync($"/api/imports/{runId}/discard", content: null);
        await EnsureSuccessAsync(response);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException($"{(int)response.StatusCode} {response.ReasonPhrase}: {body}");
    }

    public void Dispose() => _http.Dispose();
}

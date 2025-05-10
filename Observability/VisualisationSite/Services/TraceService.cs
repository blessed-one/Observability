using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Realisation;
using VisualisationSite.Models;

namespace VisualisationSite.Services;

public class TraceService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public TraceService(IOptions<StorageApiConfig> config)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(config.Value.BaseUrl)
        };
        _httpClient.DefaultRequestHeaders.Add("Storage-Key", config.Value.StorageKey);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        };
    }

    public async Task<IEnumerable<ObservabilityRecord>> GetTraces()
    {
        var response = await _httpClient.GetFromJsonAsync<IEnumerable<ObservabilityRecord>>("/records-debug", _jsonOptions);
        Console.WriteLine($"GotTraces: {response.Count()}");
        return response ?? Enumerable.Empty<ObservabilityRecord>();
    }

    public async Task<IEnumerable<ObservabilityRecord>> GetTracesByHost(string host)
    {
        var response = await _httpClient.GetFromJsonAsync<IEnumerable<ObservabilityRecord>>($"/records/host/{Uri.EscapeDataString(host)}", _jsonOptions);
        Console.WriteLine($"GotTraces: {response.Count()}");
        return response ?? Enumerable.Empty<ObservabilityRecord>();
    }

    public async Task<IEnumerable<ObservabilityRecord>> GetTracesByTraceId(string traceId)
    {
        var response = await _httpClient.GetFromJsonAsync<IEnumerable<ObservabilityRecord>>($"/records/trace/{Uri.EscapeDataString(traceId)}", _jsonOptions);
        Console.WriteLine($"GotTraces: {response.Count()}");
        return response ?? Enumerable.Empty<ObservabilityRecord>();
    }

    public async Task<IEnumerable<ObservabilityRecord>> GetTracesByParentId(string parentId)
    {
        var allTraces = await GetTraces();
        Console.WriteLine($"GotTraces: {allTraces.Count()}");
        return allTraces.Where(t => t.ParentId == parentId);
    }

    public async Task<IEnumerable<ObservabilityRecord>> GetRootTraces()
    {
        var allTraces = await GetTraces();
        Console.WriteLine($"GotTraces: {allTraces.Count()}");
        return allTraces.Where(t => t.ParentId == "0");
    }

    public async Task<IEnumerable<ObservabilityRecord>> GetTracesByTimeRange(DateTime start, DateTime end)
    {
        var response = await _httpClient.GetFromJsonAsync<IEnumerable<ObservabilityRecord>>(
            $"/records/range?start={Uri.EscapeDataString(start.ToString("o"))}&end={Uri.EscapeDataString(end.ToString("o"))}", 
            _jsonOptions);
        return response ?? Enumerable.Empty<ObservabilityRecord>();
    }

    public async Task<IEnumerable<ObservabilityRecord>> GetLastNTraces(int n)
    {
        var response = await _httpClient.GetFromJsonAsync<IEnumerable<ObservabilityRecord>>($"/records/last/{n}", _jsonOptions);
        return response ?? Enumerable.Empty<ObservabilityRecord>();
    }
} 
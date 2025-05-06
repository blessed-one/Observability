using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Realisation;

public class ObservabilityActivity
{
    private ObservabilityRecord Record { get; }
    private readonly Stopwatch _stopWatch;
    
    public ObservabilityActivity(string nodeId, HttpContext httpContext)
    {
        _stopWatch = Stopwatch.StartNew();
        Record = new ObservabilityRecord
        {
            TraceId = httpContext.TraceIdentifier,
            ParentId = "0",
            NodeId = nodeId,
            Timestamp = DateTime.UtcNow,
            HttpRequestData = [],
            MetricData = [],
            Message = null,
            IsError = false
        };
        AddFromRequest(httpContext.Request);
    }

    private void AddFromRequest(HttpRequest request)
    {
        var method = request.Method;
        var parentId = request.Headers.TryGetValue("traceparent", out var parentIdValue)
            ? parentIdValue.ToString()
            : "0";
        var path = request.Path.ToString();
        
        Record.ParentId = parentId;
        Record.HttpRequestData["method"] = method;
        Record.HttpRequestData["path"] = path;
        
        foreach (var header in request.Headers)
        {
            Record.HttpRequestData[$"header:{header.Key}"] = header.Value.ToString();
        }
    }
    
    public void Start() => _stopWatch.Start();

    public void Stop()
    {
        _stopWatch.Stop();
        Record.MetricData = new Dictionary<string, object>
        {
            ["duration"] = _stopWatch.ElapsedMilliseconds,
            ["memory"] = GC.GetTotalMemory(false),
            ["cpu"] = Process.GetCurrentProcess().TotalProcessorTime
        };
    }

    public void Error(string message)
    {
        Record.Message = message;
        Record.IsError = true;
    }

    public string GetActivityJson() => JsonSerializer.Serialize(Record);
}
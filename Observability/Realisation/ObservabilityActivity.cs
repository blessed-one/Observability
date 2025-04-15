using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Realisation;

public class ObservabilityActivity
{
    private ObservabilityRecord Record { get; }
    private readonly Activity _activity;
    private readonly Stopwatch _stopWatch;
    
    public ObservabilityActivity(Activity activity, string nodeId, HttpContext httpContext)
    {
        _activity = activity;
        _stopWatch = Stopwatch.StartNew();
        Record = new ObservabilityRecord
        {
            TraceId = Activity.Current!.TraceId.ToString(),
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
        var headers = request.Headers;
        var parentId = headers.TryGetValue("traceparent", out var parentIdValue)
            ? parentIdValue.ToString()
            : "0";
        headers.Remove("traceparent");
        
        Record.ParentId = parentId;
        Record.HttpRequestData["method"] = method;
        Record.HttpRequestData["headers"] = headers.ToDictionary(
            h => h.Key, 
            h => h.Value.ToString());
    }
    
    public string GetParentId() => Record.ParentId; 

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
        _activity.Stop();
    }

    public void Error(string message)
    {
        Record.Message = message;
        Record.IsError = true;
    }

    public string GetActivityJson() => JsonSerializer.Serialize(Record);
}
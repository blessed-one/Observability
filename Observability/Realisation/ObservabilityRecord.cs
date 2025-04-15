namespace Realisation;

public class ObservabilityRecord
{
    public required string TraceId { get; set; }
    public required string ParentId { get; set; }
    public required Dictionary<string, object> HttpRequestData { get; set; }
    public required Dictionary<string, object>? MetricData { get; set; }
    public required string NodeId { get; set; }
    public required DateTime Timestamp { get; set; }
    public required string? Message { get; set; }
    public required bool IsError { get; set; }
}

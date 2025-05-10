using Realisation;

namespace VisualisationSite.Models;

public static class ObservabilityRecordExtensions
{
    public static string GetHost(this ObservabilityRecord record)
    {
        return record.HttpRequestData.TryGetValue("header:Host", out var host) ? host.ToString() ?? string.Empty : string.Empty;
    }

    public static string GetMethod(this ObservabilityRecord record)
    {
        return record.HttpRequestData.TryGetValue("method", out var method) ? method.ToString() ?? string.Empty : string.Empty;
    }

    public static string GetPath(this ObservabilityRecord record)
    {
        return record.HttpRequestData.TryGetValue("path", out var path) ? path.ToString() ?? string.Empty : string.Empty;
    }

    public static double GetDuration(this ObservabilityRecord record)
    {
        if (record.MetricData != null && record.MetricData.TryGetValue("duration", out var duration))
        {
            return Convert.ToDouble(duration);
        }
        return 0;
    }

    public static long GetMemory(this ObservabilityRecord record)
    {
        if (record.MetricData != null && record.MetricData.TryGetValue("memory", out var memory))
        {
            return Convert.ToInt64(memory);
        }
        return 0;
    }

    public static string GetCpu(this ObservabilityRecord record)
    {
        if (record.MetricData != null && record.MetricData.TryGetValue("cpu", out var cpu))
        {
            return cpu.ToString() ?? string.Empty;
        }
        return string.Empty;
    }
} 
using System.Text.Json;
using MongoDB.Bson;
using Realisation;

namespace Storage;

public static class RecordMapper
{
    public static RecordEntity ToEntity(this ObservabilityRecord record)
    {
        return new RecordEntity
        {
            TraceId = record.TraceId,
            ParentId = record.ParentId,
            HttpRequestData = BsonDocument.Parse(JsonSerializer.Serialize(record.HttpRequestData)),
            MetricData = record.MetricData != null 
                ? BsonDocument.Parse(JsonSerializer.Serialize(record.MetricData))
                : null,
            NodeId = record.NodeId,
            Timestamp = record.Timestamp,
            Message = record.Message,
            IsError = record.IsError
        };
    }

    public static ObservabilityRecord ToModel(this RecordEntity entity)
    {
        return new ObservabilityRecord
        {
            TraceId = entity.TraceId,
            ParentId = entity.ParentId,
            HttpRequestData = entity.HttpRequestData.ToDictionary(),
            MetricData = entity.MetricData?.ToDictionary(),
            NodeId = entity.NodeId,
            Timestamp = entity.Timestamp,
            Message = entity.Message,
            IsError = entity.IsError
        };
    }
}
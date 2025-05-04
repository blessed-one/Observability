using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Storage;

[BsonIgnoreExtraElements]
public class RecordEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("traceId")]
    public required string TraceId { get; set; }

    [BsonElement("parentId")]
    public required string ParentId { get; set; }

    [BsonElement("httpRequestData")]
    public required BsonDocument HttpRequestData { get; set; }

    [BsonElement("metricData")]
    public required BsonDocument? MetricData { get; set; }

    [BsonElement("nodeId")]
    public required string NodeId { get; set; }

    [BsonElement("timestamp")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public required DateTime Timestamp { get; set; }

    [BsonElement("message")]
    public required string? Message { get; set; }

    [BsonElement("isError")]
    public required bool IsError { get; set; }
}
using MongoDB.Driver;
using MongoDB.Bson;
using Realisation;
using Storage;

namespace Storage.Services;

public class MongoService(IMongoCollection<RecordEntity> collection)
{
    public async Task<RecordEntity> AddRecordAsync(ObservabilityRecord record)
    {
        var entity = record.ToEntity();
        await collection.InsertOneAsync(entity);
        
        return entity;
    }

    public async Task<IEnumerable<ObservabilityRecord>> GetAllRecordsAsync()
    {
        var entities = await collection.Find(_ => true).ToListAsync();
        var models = entities.Select(entity => entity.ToModel()).ToList();
        
        return models;
    }

    public async Task<IEnumerable<ObservabilityRecord>> GetLastNRecordsAsync(int n)
    {
        var entities = await collection.Find(_ => true)
            .Sort(Builders<RecordEntity>.Sort.Descending("_id"))
            .Limit(n)
            .ToListAsync();
        var models = entities.Select(entity => entity.ToModel()).ToList();
        
        return models;
    }

    public async Task<IEnumerable<ObservabilityRecord>> GetRecordsByTraceIdAsync(string traceId)
    {
        var entities = await collection.Find(r => r.TraceId == traceId)
            .Sort(Builders<RecordEntity>.Sort.Ascending("Timestamp"))
            .ToListAsync();
        var models = entities.Select(entity => entity.ToModel()).ToList();
        
        return models;
    }

    public async Task<IEnumerable<ObservabilityRecord>> GetRecordsByTimeRangeAsync(DateTime start, DateTime end)
    {
        var entities = await collection.Find(r => r.Timestamp >= start && r.Timestamp <= end)
            .Sort(Builders<RecordEntity>.Sort.Ascending("Timestamp"))
            .ToListAsync();
        var models = entities.Select(entity => entity.ToModel()).ToList();
        
        return models;
    }

    public async Task<IEnumerable<ObservabilityRecord>> GetRecordsByHostAsync(string host)
    {
        var filter = Builders<RecordEntity>.Filter.Regex(
            "HttpRequestData.header:Host",
            new BsonRegularExpression(host, "i")
        );
        
        var entities = await collection.Find(filter)
            .Sort(Builders<RecordEntity>.Sort.Ascending("Timestamp"))
            .ToListAsync();
        var models = entities.Select(entity => entity.ToModel()).ToList();
        
        return models;
    }
} 
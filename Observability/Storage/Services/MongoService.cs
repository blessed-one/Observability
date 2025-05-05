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
} 
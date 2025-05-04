using FluentValidation;
using FluentValidation.AspNetCore;
using MongoDB.Driver;
using Realisation;
using Storage.Services;
using Storage.Validators;

namespace Storage.Extensions;

public static class ServicesExtensions
{
    public static void AddMongoDb(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));

        services.AddSingleton(serviceProvider =>
        {
            var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
            var database = mongoClient.GetDatabase("ObservabilityDB");
            return database.GetCollection<RecordEntity>("Records");
        });
        
        services.AddScoped<MongoService>();
    }

    public static void AddValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation(config =>
        {
            config.DisableDataAnnotationsValidation = true;
        });
        services.AddScoped<IValidator<ObservabilityRecord>, ObservabilityRecordValidator>();
    }
}
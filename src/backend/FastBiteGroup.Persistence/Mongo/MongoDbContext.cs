using FastBiteGroup.Persistence.DependencyInjection.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FastBiteGroup.Persistence.Mongo;

public sealed class MongoDbContext(IMongoClient client, IOptions<MongoDbOptions> options)
{
    private readonly IMongoDatabase _database = client.GetDatabase(options.Value.DatabaseName);

    public IMongoDatabase Database => _database;

    public IMongoCollection<TDocument> GetCollection<TDocument>(string collectionName)
        => _database.GetCollection<TDocument>(collectionName);
}

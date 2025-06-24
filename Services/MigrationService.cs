using System;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDBMigration.Models;

namespace MongoDBMigration.Services;

public class MigrationService
{
    private readonly IMongoCollection<Book> _booksCollection;

    public MigrationService(IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
    {
        var mongoClient = new MongoClient(bookStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(bookStoreDatabaseSettings.Value.DatabaseName);

        _booksCollection = mongoDatabase.GetCollection<Book>(
            bookStoreDatabaseSettings.Value.BooksCollectionName
        );
    }

    public async Task AddIndexAsync(string indexName, string fieldName)
    {
        var indexKeys = Builders<Book>.IndexKeys.Ascending(fieldName);
        var indexModel = new CreateIndexModel<Book>(
            indexKeys,
            new CreateIndexOptions { Name = indexName }
        );

        await _booksCollection.Indexes.CreateOneAsync(indexModel);
    }
}

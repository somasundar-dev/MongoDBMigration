using System;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDBMigration.Models;

namespace MongoDBMigration.Services;

public class BooksService
{
    private readonly IMongoCollection<Book> _booksCollection;

    public BooksService(IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
    {
        var mongoClient = new MongoClient(bookStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(bookStoreDatabaseSettings.Value.DatabaseName);

        _booksCollection = mongoDatabase.GetCollection<Book>(
            bookStoreDatabaseSettings.Value.BooksCollectionName
        );
    }

    public async Task<List<Book>> GetAsync() =>
        await _booksCollection.Find(_ => true).ToListAsync();

    public async Task<Book?> GetAsync(string id) =>
        await _booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Book newBook)
    {
        int retryCount = 0;
        const int maxRetries = 3;
        while (true)
        {
            try
            {
                if (retryCount > 0)
                {
                    newBook.BookName = $"{newBook.BookName} (Retry {retryCount})";
                }
                await _booksCollection.InsertOneAsync(newBook);
                break;
            }
            catch (MongoWriteException ex)
            {
                if (ex?.WriteError?.Category == ServerErrorCategory.DuplicateKey)
                {
                    retryCount++;
                    if (retryCount > maxRetries)
                    {
                        Console.WriteLine(
                            $"Duplicate key error after {maxRetries} retries: {ex.Message}"
                        );
                        throw new InvalidOperationException(
                            "A book with the same ID already exists after retries.",
                            ex
                        );
                    }
                    Console.WriteLine(
                        $"Duplicate key error, retrying {retryCount}/{maxRetries}: {ex.Message}"
                    );
                    // Optionally, update newBook.Id here if you want to generate a new ID before retrying
                    continue;
                }
                Console.WriteLine($"Error inserting book: {ex?.Message ?? "Unknown error"}");
                throw;
            }
        }
    }

    public async Task UpdateAsync(string id, Book updatedBook) =>
        await _booksCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

    public async Task RemoveAsync(string id) =>
        await _booksCollection.DeleteOneAsync(x => x.Id == id);
}

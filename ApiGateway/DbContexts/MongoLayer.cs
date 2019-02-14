using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedArea.Notifications;

namespace ApiGateway.DbContexts
{
    public class MongoLayer : IDisposable
    {
        private static IMongoDatabase _db;
        
        public static void Setup()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            _db = client.GetDatabase("ApiGatewayMongoDb");
            if (!CollectionExistsAsync("Notifications").Result)
                _db.CreateCollection("Notifications");
            Console.WriteLine("Connected to MongoDb");
        }
        
        public IMongoCollection<Notification> GetNotifsColl()
        {
            return _db.GetCollection<Notification>("Notifications");
        }
        
        private static async Task<bool> CollectionExistsAsync(string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collections = await _db.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
            return await collections.AnyAsync();
        }

        public void Dispose()
        {
            
        }
    }
}
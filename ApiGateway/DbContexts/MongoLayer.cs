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
        private static IMongoCollection<BsonDocument> _notifColl;
        private static IMongoCollection<Notification> _notifColl2;
        
        public static void Setup()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            _db = client.GetDatabase("ApiGatewayMongoDb");
            if (!CollectionExistsAsync("Notifications").Result)
                _db.CreateCollection("Notifications");
            _notifColl = _db.GetCollection<BsonDocument>("Notifications");
            _notifColl2 = _db.GetCollection<Notification>("Notifications");
            Console.WriteLine("Connected to MongoDb");
        }
        
        public IMongoCollection<BsonDocument> GetNotifsColl()
        {
            return _notifColl;
        }

        public IMongoCollection<Notification> GetNotifsColl2()
        {
            return _notifColl2;
        }
        
        private static async Task<bool> CollectionExistsAsync(string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collections = await _db.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter }).ConfigureAwait(false);
            return await collections.AnyAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            
        }
    }
}
using DEA.Core;
using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;

namespace DEA.Tools.MessageStore.MongoDb
{
    public class MongoDbMessageStore : MessageStoreBase
    {
        private string _connectionString;
        private string _databaseName;

        private MongoClient _mongoClient;
        private IMongoDatabase _mongoDatabase;

        public MongoDbMessageStore(string connectionString, string databaseName)
        {
            _connectionString = connectionString;
            _databaseName = databaseName;
        }

        public override void Begin()
        {
            _mongoClient = new MongoClient(_connectionString);
            _mongoDatabase = _mongoClient.GetDatabase(_databaseName);
        }

        public override byte[] PopMessage(string eventName, Guid messageID)
        {
            var hashCode = (uint)eventName.GetHashCode();
            var collName = $"MessageStore_{hashCode}";

            var collection = _mongoDatabase.GetCollection<BsonDocument>(collName);

            var idBin = new BsonBinaryData(messageID, GuidRepresentation.CSharpLegacy);

            var builder = new FilterDefinitionBuilder<BsonDocument>();
            var filter = builder.Eq("ID", idBin);

            var bsonDoc = collection.FindOneAndDelete(filter);
            if (bsonDoc == null)
                return null;

            if (!bsonDoc.Contains("Data"))
                return null;

            var binData = bsonDoc["Data"];
            if (!binData.IsBsonBinaryData)
                return null;

            var data = binData.AsByteArray;
            return data;

        }
        public override async Task<byte[]> PopMessageAsync(string eventName, Guid messageID)
        {
            var hashCode = (uint)eventName.GetHashCode();
            var collName = $"MessageStore_{hashCode}";

            var collection = _mongoDatabase.GetCollection<BsonDocument>(collName);

            var idBin = new BsonBinaryData(messageID, GuidRepresentation.CSharpLegacy);

            var builder = new FilterDefinitionBuilder<BsonDocument>();
            var filter = builder.Eq("ID", idBin);

            var bsonDoc = await collection.FindOneAndDeleteAsync(filter);
            if (bsonDoc == null)
                return null;

            if (!bsonDoc.Contains("Data"))
                return null;

            var binData = bsonDoc["Data"];
            if (!binData.IsBsonBinaryData)
                return null;

            var data = binData.AsByteArray;
            return data;
        }

        public override byte[] GetMessage(string eventName, Guid messageID)
        {
            var hashCode = (uint)eventName.GetHashCode();
            var collName = $"MessageStore_{hashCode}";

            var collection = _mongoDatabase.GetCollection<BsonDocument>(collName);

            var idBin = new BsonBinaryData(messageID, GuidRepresentation.CSharpLegacy);

            var builder = new FilterDefinitionBuilder<BsonDocument>();
            var filter = builder.Eq("ID", idBin);

            var cursor = collection.Find(filter);

            var bsonDoc = cursor.FirstOrDefault();
            if (bsonDoc == null)
                return null;

            if (!bsonDoc.Contains("Data"))
                return null;

            var binData = bsonDoc["Data"];
            if (!binData.IsBsonBinaryData)
                return null;

            var data = binData.AsByteArray;
            return data;
        }
        public override async Task<byte[]> GetMessageAsync(string eventName, Guid messageID)
        {
            var hashCode = (uint)eventName.GetHashCode();
            var collName = $"MessageStore_{hashCode}";

            var collection = _mongoDatabase.GetCollection<BsonDocument>(collName);

            var idBin = new BsonBinaryData(messageID, GuidRepresentation.CSharpLegacy);

            var builder = new FilterDefinitionBuilder<BsonDocument>();
            var filter = builder.Eq("ID", idBin);

            var cursor = await collection.FindAsync(filter);

            var bsonDoc = await cursor.FirstOrDefaultAsync();
            if (bsonDoc == null)
                return null;

            if (!bsonDoc.Contains("Data"))
                return null;

            var binData = bsonDoc["Data"];
            if (!binData.IsBsonBinaryData)
                return null;

            var data = binData.AsByteArray;
            return data;
        }

        public override void SaveMessage(string eventName, Guid messageID, byte[] messageData)
        {
            var hashCode = (uint)eventName.GetHashCode();
            var collName = $"MessageStore_{hashCode}";

            var collection = _mongoDatabase.GetCollection<BsonDocument>(collName);

            var bsonDoc = new BsonDocument
            {
                ["ID"] = new BsonBinaryData(messageID, GuidRepresentation.CSharpLegacy),
                ["Data"] = new BsonBinaryData(messageData, BsonBinarySubType.Binary)
            };

            collection.InsertOne(bsonDoc);
        }
        public override async Task SaveMessageAsync(string eventName, Guid messageID, byte[] messageData)
        {
            var hashCode = (uint)eventName.GetHashCode();
            var collName = $"MessageStore_{hashCode}";

            var collection = _mongoDatabase.GetCollection<BsonDocument>(collName);

            var bsonDoc = new BsonDocument
            {
                ["ID"] = new BsonBinaryData(messageID, GuidRepresentation.CSharpLegacy),
                ["Data"] = new BsonBinaryData(messageData, BsonBinarySubType.Binary)
            };

            await collection.InsertOneAsync(bsonDoc);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }
    }
}

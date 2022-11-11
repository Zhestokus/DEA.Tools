using DEA.Core;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace DEA.Tools.MessageStore.Redis
{
    public class RedisMessageStore : MessageStoreBase
    {
        private String _connectionString;

        private IConnectionMultiplexer _connection;
        private IDatabase _database;

        public RedisMessageStore(String connectionString)
        {
            _connectionString = connectionString;
        }

        public override void Begin()
        {
            _connection?.Dispose();

            _connection = ConnectionMultiplexer.Connect(_connectionString);
            _database = _connection.GetDatabase();
        }

        public override byte[] PopMessage(string eventName, Guid messageID)
        {
            var messageKey = Convert.ToString(messageID);

            var redisValue = _database.HashGet(eventName, messageKey);
            _database.HashDelete(eventName, messageKey);

            if (!redisValue.HasValue || redisValue.IsNull)
                return null;

            var messageData = (byte[])redisValue;
            return messageData;
        }
        public override async Task<byte[]> PopMessageAsync(string eventName, Guid messageID)
        {
            var messageKey = Convert.ToString(messageID);

            var redisValue = await _database.HashGetAsync(eventName, messageKey);
            await _database.HashDeleteAsync(eventName, messageKey);

            if (!redisValue.HasValue || redisValue.IsNull)
                return null;

            var messageData = (byte[])redisValue;
            return messageData;
        }

        public override byte[] GetMessage(string eventName, Guid messageID)
        {
            var messageKey = Convert.ToString(messageID);

            var redisValue = _database.HashGet(eventName, messageKey);
            if (!redisValue.HasValue || redisValue.IsNull)
                return null;

            var messageData = (byte[])redisValue;
            return messageData;
        }
        public override async Task<byte[]> GetMessageAsync(string eventName, Guid messageID)
        {
            var messageKey = Convert.ToString(messageID);

            var redisValue = await _database.HashGetAsync(eventName, messageKey);
            if (!redisValue.HasValue || redisValue.IsNull)
                return null;

            var messageData = (byte[])redisValue;
            return messageData;
        }

        public override void SaveMessage(string eventName, Guid messageID, byte[] messageData)
        {
            var messageKey = Convert.ToString(messageID);
            _database.HashSet(eventName, messageKey, messageData);
        }
        public override async Task SaveMessageAsync(string eventName, Guid messageID, byte[] messageData)
        {
            var messageKey = Convert.ToString(messageID);
            await _database.HashSetAsync(eventName, messageKey, messageData);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _connection?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }
    }
}

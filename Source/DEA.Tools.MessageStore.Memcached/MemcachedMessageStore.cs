using DEA.Core;
using System;
using System.Threading.Tasks;
using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;

namespace DEA.Tools.MessageStore.Memcached
{
    public class MemcachedMessageStore : MessageStoreBase
    {
        private string _connectionString;

        private MemcachedClientConfiguration _cfg;
        private IMemcachedClient _memcached;

        public MemcachedMessageStore(string connectionString, TimeSpan receiveTimeout, TimeSpan deadTimeout)
        {
            _connectionString = connectionString;

            _cfg = new MemcachedClientConfiguration();

            _cfg.SocketPool.ReceiveTimeout = receiveTimeout;
            _cfg.SocketPool.DeadTimeout = deadTimeout;

            _cfg.AddServer(_connectionString);
        }

        public override void Begin()
        {
            _memcached?.Dispose();

            _memcached = new MemcachedClient(_cfg);
        }

        public override byte[] PopMessage(string eventName, Guid messageID)
        {
            var messageKey = $"{eventName}_{messageID}";

            if (!_memcached.TryGet(messageKey, out var value))
                return null;

            _memcached.Remove(messageKey);

            var data = (byte[])value;
            return data;

        }
        public override async Task<byte[]> PopMessageAsync(string eventName, Guid messageID)
        {
            var messageKey = $"{eventName}_{messageID}";

            var result = await Task.Run(() =>
            {
                if (!_memcached.TryGet(messageKey, out var value))
                    return null;

                _memcached.Remove(messageKey);

                var data = (byte[])value;
                return data;
            });

            return result;
        }

        public override byte[] GetMessage(string eventName, Guid messageID)
        {
            var messageKey = $"{eventName}_{messageID}";



            if (!_memcached.TryGet(messageKey, out var value))
                return null;

            var data = (byte[])value;
            return data;

        }
        public override async Task<byte[]> GetMessageAsync(string eventName, Guid messageID)
        {
            var messageKey = $"{eventName}_{messageID}";

            var result = await Task.Run(() =>
            {
                if (!_memcached.TryGet(messageKey, out var value))
                    return null;

                var data = (byte[])value;
                return data;
            });

            return result;
        }

        public override void SaveMessage(string eventName, Guid messageID, byte[] messageData)
        {
            var messageKey = $"{eventName}_{messageID}";
            _memcached.Store(StoreMode.Set, messageKey, messageData);
        }
        public override async Task SaveMessageAsync(string eventName, Guid messageID, byte[] messageData)
        {
            var messageKey = $"{eventName}_{messageID}";
            await Task.Run(() => _memcached.Store(StoreMode.Set, messageKey, messageData));
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _memcached?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }
    }
}

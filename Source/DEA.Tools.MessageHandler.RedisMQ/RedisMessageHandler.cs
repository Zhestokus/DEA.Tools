using DEA.Core;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DEA.Tools.MessageHandler.RedisMQ
{
    public class RedisMqMessageHandler : MessageHandlerBase
    {
        private String _connectionString;

        private IDictionary<String, ChannelMessageQueue> _channelQueues;

        private IConnectionMultiplexer _connection;
        private ISubscriber _subcriber;
        private IDatabase _database;

        private int _processing;

        public RedisMqMessageHandler(String connectionString)
        {
            _connectionString = connectionString;
            _channelQueues = new Dictionary<String, ChannelMessageQueue>();
        }

        public override void Begin(Func<String, byte[], bool> handler)
        {
            base.Begin(handler);

            _connection?.Dispose();

            _connection = ConnectionMultiplexer.Connect(_connectionString);
            _subcriber = _connection.GetSubscriber();
            _database = _connection.GetDatabase();
        }

        public override void Send(String channel, byte[] data)
        {
            var queueName = $"{channel}_Queue";

            _database.ListRightPush(queueName, data);
            _subcriber.Publish(channel, data);
        }

        public override async Task SendAsync(String channel, byte[] data)
        {
            var queueName = $"{channel}_Queue";

            await _database.ListRightPushAsync(queueName, data);
            await _subcriber.PublishAsync(channel, "@");
        }

        public override bool Subscribe(String channel)
        {
            lock (_channelQueues)
            {
                if (_channelQueues.ContainsKey(channel))
                    return false;

                var channelQueue = _subcriber.Subscribe(channel);
                _channelQueues[channel] = channelQueue;

                channelQueue.OnMessage(OnMessage);
                return true;
            }
        }

        public override bool Unsubscribe(String channel)
        {
            lock (_channelQueues)
            {
                if (!_channelQueues.TryGetValue(channel, out var channelQueue))
                    return false;

                _channelQueues.Remove(channel);

                channelQueue.Unsubscribe();
                return true;
            }
        }

        private void OnMessage(ChannelMessage message)
        {
            var processing = Interlocked.Increment(ref _processing);
            if (processing > 1)
            {
                Interlocked.Decrement(ref _processing);
                return;
            }

            var channel = (String)message.Channel;
            var queueName = $"{channel}_Queue";

            var lastError = (Exception)null;
            while (lastError == null)
            {
                var length = _database.ListLength(queueName);
                if (length < 1)
                    break;

                var data = (byte[])_database.ListLeftPop(queueName);

                try
                {
                    OnMessage(channel, data);
                }
                catch (Exception ex)
                {
                    lastError = ex;
                }
            }

            Interlocked.Decrement(ref _processing);

            if (lastError != null)
                throw lastError;
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

using Confluent.Kafka;
using DEA.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DEA.Tools.MessageHandler.Kafka
{
    public class KafkaMessageHandler : MessageHandlerBase
    {
        private String _connectionString;

        private ISet<String> _subscribes;

        private ProducerConfig _producerConfig;
        private ConsumerConfig _consumerConfig;

        private IProducer<Null, byte[]> _producer;
        private IConsumer<Ignore, byte[]> _consumer;

        private Thread _thread;

        public KafkaMessageHandler(String connectionString)
        {
            _connectionString = connectionString;
            _subscribes = new HashSet<String>();
        }

        public override void Begin(Func<String, byte[], bool> handler)
        {
            base.Begin(handler);

            _producerConfig = new ProducerConfig
            {
                BootstrapServers = _connectionString,
            };

            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _connectionString,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                GroupId = AppDomain.CurrentDomain.FriendlyName,
            };

            _producer = new ProducerBuilder<Null, byte[]>(_producerConfig).Build();
            _consumer = new ConsumerBuilder<Ignore, byte[]>(_consumerConfig).Build();

            _thread = new Thread(StartConsume) { IsBackground = true };
            _thread.Start();
        }

        public override void Send(String channel, byte[] data)
        {
            var message = new Message<Null, byte[]>
            {
                Value = data
            };

            _producer.Produce(channel, message);
        }

        public override async Task SendAsync(String channel, byte[] data)
        {
            var message = new Message<Null, byte[]>
            {
                Value = data
            };

            await _producer.ProduceAsync(channel, message);
        }

        public override bool Subscribe(String channel)
        {
            lock (_subscribes)
            {
                if (!_subscribes.Add(channel))
                    return false;

                _consumer.Subscribe(_subscribes);
                return true;
            }
        }

        public override bool Unsubscribe(String channel)
        {
            lock (_subscribes)
            {
                if (!_subscribes.Remove(channel))
                    return false;

                _consumer.Subscribe(_subscribes);
                return true;
            }
        }

        private void StartConsume()
        {
            while (true)
            {
                var consumeResult = _consumer.Consume();
                var message = consumeResult.Message;

                var channel = consumeResult.Topic;
                var data = message.Value;

                OnMessage(channel, data);

                _consumer.Commit(consumeResult);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _producer?.Dispose();
                    _consumer?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }
    }
}

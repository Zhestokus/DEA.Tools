using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DEA.Core
{
    public abstract class MessageHandlerBase : IMessageHandler
    {
        protected Func<string, byte[], bool> _handler;

        protected bool _disposed;

        public MessageHandlerBase()
        {
        }

        public virtual void Begin(Func<string, byte[], bool> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            _handler = handler;
        }

        public abstract void Send(string channel, byte[] data);
        public abstract Task SendAsync(string channel, byte[] data);

        public abstract bool Subscribe(string channel);

        public abstract bool Unsubscribe(string channel);

        protected void OnMessage(String channel, byte[] data)
        {
            if (_handler == null)
                throw new Exception();

            _handler(channel, data);
        }

        protected virtual void Dispose(bool disposing)
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

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MessageHandlerBase()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

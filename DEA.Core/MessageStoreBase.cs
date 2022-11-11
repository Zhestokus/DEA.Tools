using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Core
{
    public abstract class MessageStoreBase : IMessageStore
    {
        protected bool _disposed;

        public MessageStoreBase()
        {
        }

        public abstract void Begin();

        public abstract byte[] PopMessage(string eventName, Guid messageID);
        public abstract Task<byte[]> PopMessageAsync(string eventName, Guid messageID);

        public abstract byte[] GetMessage(string eventName, Guid messageID);
        public abstract Task<byte[]> GetMessageAsync(string eventName, Guid messageID);

        public abstract void SaveMessage(string eventName, Guid messageID, byte[] messageData);
        public abstract Task SaveMessageAsync(string eventName, Guid messageID, byte[] messageData);

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

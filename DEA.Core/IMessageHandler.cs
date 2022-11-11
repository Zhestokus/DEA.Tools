using System;
using System.Threading.Tasks;

namespace DEA.Core
{
    public interface IMessageHandler : IDisposable
    {
        void Begin(Func<String, byte[], bool> handler);

        void Send(string channel, byte[] data);
        Task SendAsync(string channel, byte[] data);

        bool Subscribe(string channel);
        bool Unsubscribe(string channel);
    }
}

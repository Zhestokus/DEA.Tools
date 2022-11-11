using System;
using System.Threading.Tasks;

namespace DEA.Core
{
    public interface IMessageStore : IDisposable
    {
        void Begin();

        byte[] PopMessage(string eventName, Guid messageID);
        Task<byte[]> PopMessageAsync(string eventName, Guid messageID);

        byte[] GetMessage(string eventName, Guid messageID);
        Task<byte[]> GetMessageAsync(string eventName, Guid messageID);

        void SaveMessage(string eventName, Guid messageID, byte[] messageData);
        Task SaveMessageAsync(string eventName, Guid messageID, byte[] messageData);
    }
}

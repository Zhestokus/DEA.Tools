using System;

namespace DEA.Core
{
    public interface IDeaConnector : IDisposable
    {
        IDeaConnector UseStore(Func<IMessageStore> func);
        IDeaConnector UseHandler(Func<IMessageHandler> func);
        IDeaConnector UseSerializer(Func<IDataSerializer> func);
        IDeaConnector UseCompression(Func<IDataCompressor> func);

        IDeaConnector SetDefaultTimeout(TimeSpan timeout);
        IDeaConnector SetThreadingMode(ThreadingMode mode);
        IDeaConnector SetCompressionLimit(int dataLength);


        IDeaConnector MapEventHandler(String eventName, Delegate handler);
        IDeaConnector MapEventHandler(String eventName, Action<String, byte[][]> handler);
        IDeaConnector MapEventHandler(String eventName, Func<String, byte[][], Object> handler);

        IDeaConnector MapEventController(Type type);
        IDeaConnector MapEventController(Type type, bool singletone);

        IDeaConnector MapEventController<TObject>();
        IDeaConnector MapEventController<TObject>(bool singletone);

        IDeaConnector Connect();
    }
}

using System;
using System.Threading.Tasks;

namespace DEA.Core
{
    public interface IDeaProcessor : IDisposable
    {
        IDeaProcessor UseStore(Func<IMessageStore> func);
        IDeaProcessor UseHandler(Func<IMessageHandler> func);
        IDeaProcessor UseSerializer(Func<IDataSerializer> func);
        IDeaProcessor UseCompression(Func<IDataCompressor> func);

        IDeaProcessor SetDefaultTimeout(TimeSpan timeout);
        IDeaProcessor SetThreadingMode(ThreadingMode mode);
        IDeaProcessor SetHostIdentifier(String hostIdentifier);
        IDeaProcessor SetCompressionLimit(int dataLength);

        IDeaProcessor Connect();

        void Process();
        void Process(String eventName);

        void Process(params Object[] @params);
        void Process(String eventName, params Object[] @params);

        Task ProcessAsync();
        Task ProcessAsync(String eventName);

        Task ProcessAsync(params Object[] @params);
        Task ProcessAsync(String eventName, params Object[] @params);

        Object Process(Type answerType);
        Object Process(String eventName, Type answerType);

        Object Process(Type answerType, params Object[] @params);
        Object Process(String eventName, Type answerType, params Object[] @params);

        Task<Object> ProcessAsync(Type answerType);
        Task<Object> ProcessAsync(String eventName, Type answerType);

        Task<Object> ProcessAsync(Type answerType, params Object[] @params);
        Task<Object> ProcessAsync(String eventName, Type answerType, params Object[] @params);

        TResult Process<TResult>();
        TResult Process<TResult>(String eventName);

        TResult Process<TResult>(params Object[] @params);
        TResult Process<TResult>(String eventName, params Object[] @params);

        Task<TResult> ProcessAsync<TResult>();
        Task<TResult> ProcessAsync<TResult>(String eventName);

        Task<TResult> ProcessAsync<TResult>(params Object[] @param);
        Task<TResult> ProcessAsync<TResult>(String eventName, params Object[] @param);
    }
}

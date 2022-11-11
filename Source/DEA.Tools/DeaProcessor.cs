using DEA.Core;
using DEA.Tools.Helpers;
using DEA.Tools.Models;
using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DEA.Tools
{
    public class DeaProcessor : IDeaProcessor
    {
        private bool _disposed;

        private int _compressionLimit;
        private String _hostIdentifier;

        private TimeSpan _defaultTimeout;
        private ThreadingMode _threadingMode;

        private IDataSerializer _dataSerializer;
        private IDataCompressor _dataCompressor;

        private IMessageStore _messageStore;
        private IMessageHandler _messageHandler;

        private IDictionary<Guid, EventContent> _eventContents;

        private RecyclableMemoryStreamManager _recyclabeMemoryManager;

        public DeaProcessor()
        {
            _threadingMode = ThreadingMode.TaskHandled;
            _defaultTimeout = TimeSpan.FromSeconds(60);
            _hostIdentifier = GetDefaultIdentifier();
            _compressionLimit = 1024;

            _eventContents = new Dictionary<Guid, EventContent>();
            _recyclabeMemoryManager = new RecyclableMemoryStreamManager();
        }

        public IDeaProcessor SetDefaultTimeout(TimeSpan timeout)
        {
            _defaultTimeout = timeout;
            return this;
        }
        public IDeaProcessor SetThreadingMode(ThreadingMode mode)
        {
            _threadingMode = mode;
            return this;
        }
        public IDeaProcessor SetHostIdentifier(String hostIdentifier)
        {
            _hostIdentifier = hostIdentifier;
            return this;
        }
        public IDeaProcessor SetCompressionLimit(int dataLength)
        {
            _compressionLimit = dataLength;
            return this;
        }

        public IDeaProcessor UseStore(Func<IMessageStore> func)
        {
            _messageStore = func();
            return this;
        }
        public IDeaProcessor UseHandler(Func<IMessageHandler> func)
        {
            _messageHandler = func();
            return this;
        }
        public IDeaProcessor UseSerializer(Func<IDataSerializer> func)
        {
            _dataSerializer = func();
            return this;
        }
        public IDeaProcessor UseCompression(Func<IDataCompressor> func)
        {
            _dataCompressor = func();
            return this;
        }

        public IDeaProcessor Connect()
        {
            if (_messageHandler == null)
                throw new Exception();

            if (_dataSerializer == null)
                throw new Exception();

            _messageStore?.Begin();
            _messageHandler.Begin(OnMessage);

            return this;
        }

        public void Process()
        {
            var eventName = GetEventName();
            Process(eventName);
        }
        public void Process(String eventName)
        {
            Process(eventName, (Object[])null);
        }

        public void Process(params Object[] @params)
        {
            var eventName = GetEventName();
            Process(eventName, @params);
        }
        public void Process(String eventName, params Object[] @params)
        {
            Process(eventName, null, @params);
        }

        public async Task ProcessAsync()
        {
            var eventName = GetEventName();
            await ProcessAsync(eventName);
        }
        public async Task ProcessAsync(String eventName)
        {
            await ProcessAsync(eventName, (Object[])null);
        }

        public async Task ProcessAsync(params Object[] @params)
        {
            var eventName = GetEventName();
            await ProcessAsync(eventName, @params);
        }
        public async Task ProcessAsync(String eventName, params Object[] @params)
        {
            await ProcessAsync(eventName, null, @params);
        }

        public Object Process(Type answerType)
        {
            var eventName = GetEventName();
            return Process(eventName, answerType);
        }
        public Object Process(String eventName, Type answerType)
        {
            return Process(eventName, answerType, null);
        }

        public Object Process(Type answerType, params Object[] @params)
        {
            var eventName = GetEventName();
            return Process(eventName, answerType, @params);
        }
        public Object Process(String eventName, Type answerType, params Object[] @params)
        {
            return ProcessImpl(eventName, answerType, @params);
        }

        public async Task<Object> ProcessAsync(Type answerType)
        {
            var eventName = GetEventName();
            return await ProcessAsync(eventName, answerType);
        }
        public async Task<Object> ProcessAsync(String eventName, Type answerType)
        {
            return await ProcessAsync(eventName, answerType, null);
        }

        public async Task<Object> ProcessAsync(Type answerType, params Object[] @params)
        {
            var eventName = GetEventName();
            return await ProcessAsync(eventName, answerType, @params);
        }
        public async Task<Object> ProcessAsync(String eventName, Type answerType, params Object[] @params)
        {
            return await ProcessImplAsync(eventName, answerType, @params);
        }

        public TResult Process<TResult>()
        {
            var eventName = GetEventName();
            return Process<TResult>(eventName);
        }
        public TResult Process<TResult>(String eventName)
        {
            var outputObj = Process(eventName, typeof(TResult));
            if (outputObj == null)
                return default(TResult);

            return (TResult)outputObj;
        }

        public TResult Process<TResult>(params Object[] @params)
        {
            var eventName = GetEventName();
            return Process<TResult>(eventName, @params);
        }
        public TResult Process<TResult>(String eventName, params Object[] @params)
        {
            var outputObj = Process(eventName, typeof(TResult), @params);
            if (outputObj == null)
                return default(TResult);

            return (TResult)outputObj;
        }

        public async Task<TResult> ProcessAsync<TResult>()
        {
            var eventName = GetEventName();
            return await ProcessAsync<TResult>(eventName);
        }
        public async Task<TResult> ProcessAsync<TResult>(String eventName)
        {
            var outputObj = await ProcessAsync(eventName, typeof(TResult));
            if (outputObj == null)
                return default(TResult);

            return (TResult)outputObj;
        }

        public async Task<TResult> ProcessAsync<TResult>(params Object[] @param)
        {
            var eventName = GetEventName();
            return await ProcessAsync<TResult>(eventName, @param);
        }
        public async Task<TResult> ProcessAsync<TResult>(String eventName, params Object[] @param)
        {
            var outputObj = await ProcessAsync(eventName, typeof(TResult), @param);
            if (outputObj == null)
                return default(TResult);

            return (TResult)outputObj;
        }

        private Object ProcessImpl(String eventName, Type answerType, params Object[] @params)
        {
            if (_messageHandler == null)
                throw new Exception("MessageHandler is not configured");

            var requestChannel = $"{eventName}_Request";
            var responseChannel = $"{eventName}_{_hostIdentifier:n}_Response";

            _messageHandler.Subscribe(responseChannel);

            var requestModel = CreateRequestModel(eventName, null, @params);
            var requestBytes = SerializeData(requestModel);

            var eventContent = InitEventContent(requestModel, answerType);

            SendMessage(requestChannel, requestModel.RequestID, requestBytes);

            if (answerType == null || answerType == typeof(void))
                return null;

            if (!eventContent.ResponseWait.WaitOne(_defaultTimeout))
                throw new Exception("Timeout");

            if (eventContent.ReturnType == null || eventContent.ResponseData == null)
                return null;

            var responseModel = (ResponseModel)eventContent.ResponseData;
            if (responseModel.ResultData == null)
                return null;

            var outputObj = DeserializeObject(responseModel.ResultData, eventContent.ReturnType);
            return outputObj;
        }
        private async Task<Object> ProcessImplAsync(String eventName, Type answerType, params Object[] @params)
        {
            if (_messageHandler == null)
                throw new Exception("MessageHandler is not configured");

            var requestChannel = $"{eventName}_Request";
            var responseChannel = $"{eventName}_{_hostIdentifier:n}_Response";

            _messageHandler.Subscribe(responseChannel);

            var requestModel = CreateRequestModel(eventName, null, @params);
            var requestBytes = SerializeData(requestModel);

            var eventContent = InitEventContent(requestModel, answerType);

            await SendMessageAsync(requestChannel, requestModel.RequestID, requestBytes);

            if (answerType == null || answerType == typeof(void))
                return null;

            var result = await Task.Run(() => eventContent.ResponseWait.WaitOne(_defaultTimeout));
            if (!result)
                throw new Exception("Timeout");

            if (eventContent.ReturnType == null || eventContent.ResponseData == null)
                return null;

            var responseModel = (ResponseModel)eventContent.ResponseData;
            if (responseModel.ResultData == null)
                return null;

            var outputObj = DeserializeObject(responseModel.ResultData, eventContent.ReturnType);
            return outputObj;
        }

        private void SendMessage(String channelName, Guid requestID, byte[] data)
        {
            var messageData = data;

            if (_messageStore != null)
            {
                _messageStore.SaveMessage(channelName, requestID, data);
                messageData = requestID.ToByteArray();
            }

            _messageHandler.Send(channelName, messageData);
        }
        private async Task SendMessageAsync(String channelName, Guid requestID, byte[] data)
        {
            var messageData = data;

            if (_messageStore != null)
            {
                await _messageStore.SaveMessageAsync(channelName, requestID, data);
                messageData = requestID.ToByteArray();
            }

            await _messageHandler.SendAsync(channelName, messageData);
        }

        private String GetEventName()
        {
            var stackTrace = new StackTrace();

            var count = stackTrace.FrameCount;
            for (var i = 0; i < count; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame.GetMethod();

                if (!Attribute.IsDefined(method, typeof(DeaEventAttribute)))
                    continue;

                var deaAttr = (DeaEventAttribute)Attribute.GetCustomAttribute(method, typeof(DeaEventAttribute));
                if (deaAttr == null)
                    continue;

                if (!String.IsNullOrEmpty(deaAttr.EventName))
                    return deaAttr.EventName;

                var eventName = $"{method.DeclaringType.Name}.{method.Name}";
                return eventName;
            }

            throw new Exception();
        }

        private bool OnMessage(String eventName, byte[] data)
        {
            var messageData = data;
            if (_messageStore != null)
            {
                var messageID = new Guid(data);
                messageData = _messageStore.PopMessage(eventName, messageID);
            }

            var model = DeserializeData<ResponseModel>(messageData);

            var eventContent = GetEventContent(model.RequestID);
            if (eventContent == null)
                return false;

            eventContent.SetSignalled(model.SourceHost, model);
            return true;
        }

        private EventContent GetEventContent(Guid requestID)
        {
            lock (_eventContents)
            {
                if (!_eventContents.TryGetValue(requestID, out var content))
                    return null;

                return content;
            }
        }
        private EventContent InitEventContent(RequestModel model, Type returnType)
        {
            lock (_eventContents)
            {
                if (!_eventContents.TryGetValue(model.RequestID, out var content))
                {
                    content = new EventContent
                    {
                        RequestID = model.RequestID,
                        SourceHost = model.SourceHost,
                        ReturnType = returnType,
                        ResponseWait = new AutoResetEvent(false),
                    };

                    _eventContents[model.RequestID] = content;
                }

                return content;
            }
        }

        private RequestModel CreateRequestModel(String eventName, String[] generics, Object[] @params)
        {
            var parameters = (byte[][])null;

            if (@params != null)
            {
                parameters = new byte[@params.Length][];

                for (var i = 0; i < @params.Length; i++)
                {
                    if (@params[i] != null)
                        parameters[i] = SerializeObject(@params[i]);
                }
            }

            var model = new RequestModel
            {
                RequestID = Guid.NewGuid(),
                SourceHost = _hostIdentifier,
                EventName = eventName,
                Parameters = parameters
            };

            return model;
        }

        private TObject DeserializeData<TObject>(byte[] data)
        {
            var obj = DeserializeData(data, typeof(TObject));
            return (TObject)obj;
        }
        private Object DeserializeData(byte[] data, Type type)
        {
            using (var stream = new MemoryStream(data))
            {
                var reader = new BinaryReader(stream, Encoding.UTF8);

                var compressed = reader.ReadBoolean();

                var length = reader.ReadInt32();
                var source = reader.ReadBytes(length);

                if (compressed)
                {
                    if (_dataCompressor == null)
                        throw new Exception("Unable to process compressed data because compression type is not specified");

                    source = _dataCompressor.Decompress(source);
                }

                return DeserializeObject(source, type);
            }
        }
        private byte[] SerializeData(Object obj)
        {
            using (var stream = _recyclabeMemoryManager.GetStream())
            {
                var writer = new BinaryWriter(stream, Encoding.UTF8);

                var compressed = false;
                var sourceData = SerializeObject(obj);

                if (_dataCompressor != null && sourceData.Length > _compressionLimit)
                {
                    sourceData = _dataCompressor.Compress(sourceData);
                    compressed = true;
                }

                writer.Write(compressed);

                writer.Write(sourceData.Length);
                writer.Write(sourceData);

                return stream.ToArray();
            }
        }

        private Object DeserializeObject(byte[] data, Type type)
        {
            if (type.Equals(typeof(byte[])))
                return data;

            using (var stream = new MemoryStream(data))
                return _dataSerializer.Deserailize(stream, type);
        }
        private byte[] SerializeObject(Object obj)
        {
            if (obj is byte[])
                return (byte[])obj;

            using (var stream = _recyclabeMemoryManager.GetStream())
            {
                _dataSerializer.Serialize(stream, obj);

                var data = stream.ToArray();
                return data;
            }
        }

        private String GetDefaultIdentifier()
        {
            var text = $"{Environment.MachineName}_{AppDomain.CurrentDomain.FriendlyName}";

            using (var md5 = MD5Cng.Create())
            {
                var srcBytes = Encoding.UTF8.GetBytes(text);
                var hashBytes = md5.ComputeHash(srcBytes);

                var hex = hashBytes.Select(n => n.ToString("x2"));
                var hash = String.Concat(hex);

                return hash;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _messageHandler?.Dispose();
                    _dataCompressor?.Dispose();
                    _dataSerializer?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DeaProcessor()
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

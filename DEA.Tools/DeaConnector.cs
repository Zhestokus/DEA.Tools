using DEA.Core;
using DEA.Tools.Helpers;
using DEA.Tools.Models;
using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DEA.Tools
{
    public class DeaConnector : IDeaConnector
    {
        private bool _disposed;

        private int _compressionLimit;

        private TimeSpan _defaultTimeout;
        private ThreadingMode _threadingMode;

        private IDataSerializer _dataSerializer;
        private IDataCompressor _dataCompressor;

        private IMessageStore _messageStore;
        private IMessageHandler _messageHandler;

        private ISet<EventWrapper> _classHandlers;

        private IDictionary<String, Delegate> _eventHandlers;

        private RecyclableMemoryStreamManager _recyclabeMemoryManager;

        public DeaConnector()
        {
            _threadingMode = ThreadingMode.TaskHandled;
            _defaultTimeout = TimeSpan.Zero;
            _compressionLimit = 1024;

            _classHandlers = new HashSet<EventWrapper>();

            _eventHandlers = new Dictionary<String, Delegate>();
            _recyclabeMemoryManager = new RecyclableMemoryStreamManager();
        }

        public IDeaConnector SetDefaultTimeout(TimeSpan timeout)
        {
            _defaultTimeout = timeout;
            return this;
        }
        public IDeaConnector SetThreadingMode(ThreadingMode mode)
        {
            _threadingMode = mode;
            return this;
        }
        public IDeaConnector SetCompressionLimit(int dataLength)
        {
            _compressionLimit = dataLength;
            return this;
        }

        public IDeaConnector MapEventHandler(String eventName, Delegate handler)
        {
            lock (_eventHandlers)
                _eventHandlers[eventName] = handler;

            return this;
        }
        public IDeaConnector MapEventHandler(String eventName, Action<String, byte[][]> handler)
        {
            lock (_eventHandlers)
                _eventHandlers[eventName] = handler;

            return this;
        }
        public IDeaConnector MapEventHandler(String eventName, Func<String, byte[][], Object> handler)
        {
            lock (_eventHandlers)
                _eventHandlers[eventName] = handler;

            return this;
        }

        public IDeaConnector MapEventController(Type type)
        {
            return MapEventController(type, false);
        }
        public IDeaConnector MapEventController(Type type, bool singletone)
        {
            lock (_classHandlers)
            {
                var wrapper = new EventWrapper(type, singletone);
                _classHandlers.Add(wrapper);
            }

            return this;
        }

        public IDeaConnector MapEventController<TObject>()
        {
            return MapEventController(typeof(TObject), false);
        }
        public IDeaConnector MapEventController<TObject>(bool singletone)
        {
            return MapEventController(typeof(TObject), singletone);
        }

        public IDeaConnector UseStore(Func<IMessageStore> func)
        {
            _messageStore = func();
            return this;
        }
        public IDeaConnector UseHandler(Func<IMessageHandler> func)
        {
            _messageHandler = func();
            return this;
        }
        public IDeaConnector UseSerializer(Func<IDataSerializer> func)
        {
            _dataSerializer = func();
            return this;
        }
        public IDeaConnector UseCompression(Func<IDataCompressor> func)
        {
            _dataCompressor = func();
            return this;
        }

        public IDeaConnector Connect()
        {
            if (_messageHandler == null)
                throw new Exception();

            if (_dataSerializer == null)
                throw new Exception();

            _messageStore?.Begin();
            _messageHandler.Begin(OnMessage);

            lock (_eventHandlers)
            {
                lock (_classHandlers)
                {
                    foreach (var wrapper in _classHandlers)
                        wrapper.Initialize();

                    var events = new HashSet<String>();
                    events.UnionWith(_eventHandlers.Keys);

                    foreach (var wrapper in _classHandlers)
                    {
                        var eventNames = wrapper.GetEvents();
                        events.UnionWith(eventNames);
                    }

                    foreach (var eventName in events)
                    {
                        var requestChannel = $"{eventName}_Request";
                        _messageHandler.Subscribe(requestChannel);
                    }
                }
            }

            return this;
        }

        private bool OnMessage(String channelName, byte[] data)
        {
            var messageData = data;

            if (_messageStore != null)
            {
                var messageID = new Guid(data);
                messageData = _messageStore.PopMessage(channelName, messageID);
            }

            var eventName = GetEventName(channelName);
            var requestModel = DeserializeData<RequestModel>(messageData);

            var handlers = GetDelegateHandlers(eventName, requestModel);
            foreach (var handler in handlers)
                BaseHandlerProcessor(handler, eventName, requestModel);

            var classHandlers = GetClassHandlers(eventName, requestModel);
            foreach (var handler in classHandlers)
                BaseHandlerProcessor(handler, eventName, requestModel);

            return true;
        }

        private IEnumerable<Delegate> GetDelegateHandlers(String eventName, RequestModel model)
        {
            lock (_eventHandlers)
            {
                if (_eventHandlers.TryGetValue(eventName, out var @delegate))
                    yield return @delegate;

                //if (!_eventHandlers.TryGetValue(eventName, out var @set))
                //    return Enumerable.Empty<Delegate>();

                //return set.ToList();
            }
        }

        private IEnumerable<EventWrapper> GetClassHandlers(String eventName, RequestModel model)
        {
            lock (_eventHandlers)
            {
                var query = _classHandlers.Where(n => n.ContainsEvent(eventName)).ToList();
                return query.ToList();
            }
        }

        private Object BaseMessageHandler(Object handler, String eventName, byte[][] @params)
        {
            if (handler is Delegate)
                return BaseMessageHandler((Delegate)handler, eventName, @params);

            if (handler is EventWrapper)
                return BaseMessageHandler((EventWrapper)handler, eventName, @params);

            throw new Exception();
        }

        private Object BaseMessageHandler(Delegate handler, String eventName, byte[][] @params)
        {
            if (handler is Action<String, byte[][]>)
            {
                var action = (Action<String, byte[][]>)handler;

                action(eventName, @params);
                return null;
            }
            else if (handler is Func<String, byte[][], Object>)
            {
                var func = (Func<String, byte[][], Object>)handler;

                var result = func(eventName, @params);
                return result;
            }
            else
            {
                var methodInfo = handler.Method;
                var parameters = methodInfo.GetParameters();

                var paramValues = ExtractMethodParameters(eventName, parameters, @params, true);

                var result = methodInfo.Invoke(handler.Target, paramValues);
                return result;
            }
        }
        private Object BaseMessageHandler(EventWrapper wrapper, String eventName, byte[][] @params)
        {
            var paramsLen = (@params?.Length).GetValueOrDefault();

            var allParameters = wrapper.GetEventParams(eventName);
            var parameters = allParameters.FirstOrDefault(n => n.Length == paramsLen);

            var paramValues = ExtractMethodParameters(eventName, parameters, @params, false);

            var result = wrapper.InvokeEvent(eventName, paramValues);
            return result;
        }

        private Object[] ExtractMethodParameters(String eventName, ParameterInfo[] parameterInfos, byte[][] @params, bool @delegate)
        {
            var requestParamsLen = (@params?.Length).GetValueOrDefault();
            var methodParamsLen = (parameterInfos?.Length).GetValueOrDefault();

            if (@delegate)
            {
                if (parameterInfos.Length < 1)
                    throw new Exception("Delegate handler must have at least one parameter");

                var firstOne = parameterInfos[0];
                if (firstOne.ParameterType != typeof(String))
                    throw new Exception("First parameter of delegate handler should be String (to pass eventName)");

                if (requestParamsLen != (methodParamsLen - 1))
                    throw new Exception($"Invalid number of parameters of method {eventName} (number of provider params {requestParamsLen}, expected {methodParamsLen}");
            }
            else
            {
                if (requestParamsLen != methodParamsLen)
                    throw new Exception($"Invalid number of parameters of method {eventName} (number of provider params {requestParamsLen}, expected {methodParamsLen}");
            }

            //var valueIndex = 0;
            //var paramIndex = 0;

            var paramOffset = 0;
            var paramValues = new Object[methodParamsLen];

            if (@delegate)
            {
                paramValues[0] = eventName;
                paramOffset = 1;
            }

            for (var i = paramOffset; i < methodParamsLen; i++)
            {
                var paramInfo = parameterInfos[i];

                var paramName = paramInfo.Name;
                var paramType = paramInfo.ParameterType;

                var paramData = (byte[])null;
                if (@delegate)
                    paramData = @params[i - 1];
                else
                    paramData = @params[i];

                if (paramData == null || paramData.Length == 0)
                {
                    if (!IsNullable(paramType))
                        throw new Exception($"Non nullable type must have value (event: {eventName}, param: {paramName})");

                    paramValues[i] = null;
                }
                else
                {
                    paramValues[i] = DeserializeObject(paramData, paramType);
                }
            }

            return paramValues;
        }

        private void BaseHandlerProcessor(Object handler, String eventName, RequestModel requestModel)
        {
            var worker = CreateWorker()
                         .SetBaseHandler(handler)
                         .SetEventName(eventName)
                         .SetTimeout(_defaultTimeout)
                         .SetOnFinish(OnHandlerFinish)
                         .SetOnExecute(BaseMessageHandler)
                         .SetRequestModel(requestModel)
                         .Execute();
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
                var writer = new BinaryWriter(stream);

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

                return stream.ToArray();
            }
        }

        private String GetEventName(String channelName)
        {
            const String requestSuffix = "_Request";
            const String responseSuffix = "_Response";

            if (channelName.EndsWith(requestSuffix))
                return channelName.Substring(0, channelName.Length - requestSuffix.Length);

            if (channelName.EndsWith(responseSuffix))
                return channelName.Substring(0, channelName.Length - responseSuffix.Length);

            return channelName;
        }

        private void OnHandlerFinish(Object sender)
        {
            if (sender is HandlerWorkerBase)
            {
                var content = (HandlerWorkerBase)sender;

                var requestID = content.RequestID.Value;
                var sourceHost = content.SourceHost;
                var eventName = content.EventName;
                var outputObj = content.OutputObj;
                var resultData = SerializeObject(outputObj);

                var outChannel = $"{content.EventName}_{sourceHost:n}_Response";

                var model = new ResponseModel
                {
                    RequestID = requestID,
                    SourceHost = sourceHost,
                    EventName = eventName,
                    ResultData = resultData
                };

                var responseData = SerializeData(model);

                if (_messageStore != null)
                {
                    _messageStore.SaveMessage(outChannel, requestID, responseData);
                    _messageHandler.Send(outChannel, requestID.ToByteArray());
                }
                else
                {
                    _messageHandler.Send(outChannel, responseData);
                }
            }
        }

        private HandlerWorkerBase CreateWorker()
        {
            switch (_threadingMode)
            {
                case ThreadingMode.SingleThread:
                    return new SingleThreadWorker();
                case ThreadingMode.MultiThread:
                    return new MultiThreadWorker();
                case ThreadingMode.ThreadPool:
                    return new ThreadPoolWorker();
                case ThreadingMode.TaskHandled:
                    return new TaskHandledWorker();
                default:
                    throw new Exception();
            }
        }

        private bool IsNullable(Type type)
        {
            return (Nullable.GetUnderlyingType(type) != null);
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
        // ~DeaConnector()
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

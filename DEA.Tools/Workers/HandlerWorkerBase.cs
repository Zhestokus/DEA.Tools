using DEA.Tools.Models;
using System;

namespace DEA.Tools.Helpers
{
    public abstract class HandlerWorkerBase
    {
        protected Func<Object, String, byte[][], Object> _onExec;

        protected Action<Object> _onFinish;

        protected TimeSpan _execTimeout;

        protected EventWrapper _wrapper;
        protected Delegate _handler;

        protected String _eventName;

        protected Guid? _requestID;
        protected String _sourceHost;

        protected byte[][] _inputData;
        protected Object _outputObj;

        public Guid? RequestID => _requestID;

        public String SourceHost => _sourceHost;

        public String EventName => _eventName;

        public Object OutputObj => _outputObj;

        public Delegate Handler => _handler;

        public byte[][] InputData => _inputData;

        public HandlerWorkerBase SetBaseHandler(Object handler)
        {
            if (handler is Delegate)
                _handler = (Delegate)handler;

            if (handler is EventWrapper)
                _wrapper = (EventWrapper)handler;

            return this;
        }
        public HandlerWorkerBase SetBaseHandler(Delegate handler)
        {
            _handler = handler;
            return this;
        }
        public HandlerWorkerBase SetBaseHandler(EventWrapper wrapper)
        {
            _wrapper = wrapper;
            return this;
        }

        public HandlerWorkerBase SetRequestModel(RequestModel requestModel)
        {
            _requestID = requestModel.RequestID;
            _sourceHost = requestModel.SourceHost;
            _eventName = requestModel.EventName;
            _inputData = requestModel.Parameters;

            return this;
        }

        public HandlerWorkerBase SetEventName(String eventName)
        {
            _eventName = eventName;
            return this;
        }

        public HandlerWorkerBase SetSourceHost(String sourceHost)
        {
            _sourceHost = sourceHost;
            return this;
        }

        public HandlerWorkerBase SetRequestID(Guid requestID)
        {
            _requestID = requestID;
            return this;
        }

        public HandlerWorkerBase SetInputData(byte[][] inputData)
        {
            _inputData = inputData;
            return this;
        }

        public HandlerWorkerBase SetTimeout(TimeSpan timeout)
        {
            _execTimeout = timeout;
            return this;
        }

        public HandlerWorkerBase SetOnFinish(Action<Object> onFinish)
        {
            _onFinish = onFinish;
            return this;
        }

        public HandlerWorkerBase SetOnExecute(Func<Object, String, byte[][], Object> onExec)
        {
            _onExec = onExec;
            return this;
        }

        public abstract HandlerWorkerBase Execute();

        protected void VerifySetup()
        {
            if (_handler == null && _wrapper == null)
                throw new Exception();

            if (_onExec == null)
                throw new Exception();

            if (_onFinish == null)
                throw new Exception();

            if (String.IsNullOrWhiteSpace(_eventName))
                throw new Exception();

            if (_requestID == null)
                throw new Exception();
        }
    }
}

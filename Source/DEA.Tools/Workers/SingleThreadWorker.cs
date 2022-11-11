using DEA.Core;
using System;
using System.Threading;

namespace DEA.Tools.Helpers
{
    public class SingleThreadWorker : HandlerWorkerBase
    {
        public SingleThreadWorker()
        {
        }

        public override HandlerWorkerBase Execute()
        {
            VerifySetup();

            Invoke(this);

            return this;
        }

        private void Invoke(Object state)
        {
            var timer = (Timer)null;

            if (_execTimeout != TimeSpan.Zero)
            {
                var thread = Thread.CurrentThread;
                var milis = (long)_execTimeout.TotalMilliseconds;

                timer = new Timer(OnTimeout, thread, milis, Timeout.Infinite);
            }

            var handler = (Object)Handler;
            if (handler == null)
                handler = _wrapper;

            var eventName = EventName;
            var inputData = InputData;

            _outputObj = _onExec(handler, eventName, inputData);

            if (timer != null)
                timer.Dispose();

            _onFinish(this);
        }

        private void OnTimeout(Object state)
        {
            var thread = state as Thread;

            try
            {
                thread.Abort();
            }
            catch (Exception ex)
            {
                DeaLogger.LogError(ex);
            }
        }
    }

}

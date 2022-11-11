using DEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DEA.Tools.Helpers
{
    public class TaskHandledWorker : HandlerWorkerBase
    {
        private Task _task;

        public TaskHandledWorker()
        {
            _task = new Task(Invoke);
        }

        public override HandlerWorkerBase Execute()
        {
            VerifySetup();

            _task.Start();

            return this;
        }

        private void Invoke()
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

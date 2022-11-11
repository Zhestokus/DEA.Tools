using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DEA.Tools.Helpers
{
    public class EventContent : IDisposable
    {
        public Guid RequestID { get; set; }

        public String SourceHost { get; set; }

        public Type ReturnType { get; set; }

        public Object RequestData { get; set; }

        public Object ResponseData { get; set; }

        public AutoResetEvent ResponseWait { get; set; }

        public void SetSignalled()
        {
            ResponseWait.Set();
        }
        public void SetSignalled(Object responseData)
        {
            ResponseData = responseData;
            ResponseWait.Set();
        }
        public void SetSignalled(String sourceHost)
        {
            SourceHost = sourceHost;
            ResponseWait.Set();
        }
        public void SetSignalled(String sourceHost, Object responseData)
        {
            SourceHost = sourceHost;
            ResponseData = responseData;
            ResponseWait.Set();
        }

        public void Dispose()
        {
            ResponseWait?.Dispose();
        }
    }
}

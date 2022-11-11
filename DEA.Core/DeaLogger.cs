using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Core
{
    public static class DeaLogger
    {
        private static ILogger _logger;

        static DeaLogger()
        {
            _logger = new ConsoleLogger();
        }

        public static void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        public static void LogText(String text)
        {
            if (_logger != null)
                _logger.Log(text);
        }

        public static void LogError(Exception ex)
        {
            if (_logger != null)
                _logger.Log(ex);
        }
    }
}

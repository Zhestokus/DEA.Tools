using DEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.MessageHandler.RedisMQ
{
    public static class Extensions
    {
        public static IDeaConnector UseRedisMqHandler(this IDeaConnector deaConnetor, string connectionString)
        {
            var instance = deaConnetor.UseHandler(() => new RedisMqMessageHandler(connectionString));
            return instance;
        }

        public static IDeaProcessor UseRedisMqHandler(this IDeaProcessor deaProcessor, string connectionString)
        {
            var instance = deaProcessor.UseHandler(() => new RedisMqMessageHandler(connectionString));
            return instance;
        }
    }
}

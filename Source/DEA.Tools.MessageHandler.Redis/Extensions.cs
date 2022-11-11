using DEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.MessageHandler.Redis
{
    public static class Extensions
    {
        public static IDeaConnector UseRedisHandler(this IDeaConnector deaConnetor, string connectionString)
        {
            var instance = deaConnetor.UseHandler(() => new RedisMessageHandler(connectionString));
            return instance;
        }

        public static IDeaProcessor UseRedisHandler(this IDeaProcessor deaProcessor, string connectionString)
        {
            var instance = deaProcessor.UseHandler(() => new RedisMessageHandler(connectionString));
            return instance;
        }
    }
}

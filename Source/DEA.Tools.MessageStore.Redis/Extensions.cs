using DEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.MessageStore.Redis
{
    public static class Extensions
    {
        public static IDeaConnector UseRedisStore(this IDeaConnector deaConnetor, String connectionString)
        {
            var instance = deaConnetor.UseStore(() => new RedisMessageStore(connectionString));
            return instance;
        }

        public static IDeaProcessor UseRedisStore(this IDeaProcessor deaProcessor, String connectionString)
        {
            var instance = deaProcessor.UseStore(() => new RedisMessageStore(connectionString));
            return instance;
        }
    }
}

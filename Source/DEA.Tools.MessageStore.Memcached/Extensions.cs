using DEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.MessageStore.Memcached
{
    public static class Extensions
    {
        public static IDeaConnector UseMemcachedStore(this IDeaConnector deaConnetor, string connectionString)
        {
            return UseMemcachedStore(deaConnetor, connectionString, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }
        public static IDeaConnector UseMemcachedStore(this IDeaConnector deaConnetor, string connectionString, TimeSpan receiveTimeout, TimeSpan deadTimeout)
        {
            var instance = deaConnetor.UseStore(() => new MemcachedMessageStore(connectionString, receiveTimeout, deadTimeout));
            return instance;
        }

        public static IDeaProcessor UseMemcachedStore(this IDeaProcessor deaProcessor, string connectionString)
        {
            return UseMemcachedStore(deaProcessor, connectionString, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }
        public static IDeaProcessor UseMemcachedStore(this IDeaProcessor deaProcessor, string connectionString, TimeSpan receiveTimeout, TimeSpan deadTimeout)
        {
            var instance = deaProcessor.UseStore(() => new MemcachedMessageStore(connectionString, receiveTimeout, deadTimeout));
            return instance;
        }
    }
}

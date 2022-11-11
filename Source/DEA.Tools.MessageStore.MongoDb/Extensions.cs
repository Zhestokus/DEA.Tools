using DEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.MessageStore.MongoDb
{
    public static class Extensions
    {
        public static IDeaConnector UseMemcachedStore(this IDeaConnector deaConnetor, string connectionString, string databaseName)
        {
            var instance = deaConnetor.UseStore(() => new MongoDbMessageStore(connectionString, databaseName));
            return instance;
        }

        public static IDeaProcessor UseQuickLzCompression(this IDeaProcessor deaProcessor, string connectionString, string databaseName)
        {
            var instance = deaProcessor.UseStore(() => new MongoDbMessageStore(connectionString, databaseName));
            return instance;
        }
    }
}

using DEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.MessageHandler.Kafka
{
    public static class Extensions
    {
        public static IDeaConnector UseKafkaHandler(this IDeaConnector deaConnetor, String connectionString)
        {
            var instance = deaConnetor.UseHandler(() => new KafkaMessageHandler(connectionString));
            return instance;
        }

        public static IDeaProcessor UseKafkaHandler(this IDeaProcessor deaProcessor, String connectionString)
        {
            var instance = deaProcessor.UseHandler(() => new KafkaMessageHandler(connectionString));
            return instance;
        }
    }
}

using DEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.MessageHandler.RabbitMQ
{
    public static class Extensions
    {
        public static IDeaConnector UseRabbitMqHandler(this IDeaConnector deaConnetor, string connectionString)
        {
            var instance = deaConnetor.UseHandler(() => new RabbitMqMessageHandler(connectionString));
            return instance;
        }

        public static IDeaProcessor UseRabbitMqHandler(this IDeaProcessor deaProcessor, string connectionString)
        {
            var instance = deaProcessor.UseHandler(() => new RabbitMqMessageHandler(connectionString));
            return instance;
        }
    }
}

using DEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.Serialization.JsonNet
{
    public static class Extensions
    {
        public static IDeaConnector UseJsonNetSerializer(this IDeaConnector deaConnetor)
        {
            var instance = deaConnetor.UseSerializer(() => new JsonNetSerializer());
            return instance;
        }

        public static IDeaProcessor UseJsonNetSerializer(this IDeaProcessor deaProcessor)
        {
            var instance = deaProcessor.UseSerializer(() => new JsonNetSerializer());
            return instance;
        }
    }
}

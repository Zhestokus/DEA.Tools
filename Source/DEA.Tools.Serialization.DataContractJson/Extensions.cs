using DEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.Serialization.DataContractJson
{
    public static class Extensions
    {
        public static IDeaConnector UseDataContractJsonSerializer(this IDeaConnector deaConnetor)
        {
            var instance = deaConnetor.UseSerializer(() => new DataContractJsonSerializer());
            return instance;
        }

        public static IDeaProcessor UseDataContractJsonSerializer(this IDeaProcessor deaProcessor)
        {
            var instance = deaProcessor.UseSerializer(() => new DataContractJsonSerializer());
            return instance;
        }
    }
}

using DEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.Serialization.DataContractXml
{
    public static class Extensions
    {
        public static IDeaConnector UseDataContractXmlSerializer(this IDeaConnector deaConnetor)
        {
            var instance = deaConnetor.UseSerializer(() => new DataContractXmlSerializer());
            return instance;
        }

        public static IDeaProcessor UseDataContractXmlSerializer(this IDeaProcessor deaProcessor)
        {
            var instance = deaProcessor.UseSerializer(() => new DataContractXmlSerializer());
            return instance;
        }
    }
}

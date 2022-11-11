using DEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.Serialization.XmlStd
{
    public static class Extensions
    {
        public static IDeaConnector UseXmlSerializer(this IDeaConnector deaConnetor)
        {
            var instance = deaConnetor.UseSerializer(() => new StandardXmlSerializer());
            return instance;
        }

        public static IDeaProcessor UseXmlSerializer(this IDeaProcessor deaProcessor)
        {
            var instance = deaProcessor.UseSerializer(() => new StandardXmlSerializer());
            return instance;
        }
    }
}

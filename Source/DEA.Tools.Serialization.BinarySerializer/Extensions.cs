using DEA.Core;
using DEA.Tools.Serialization.BinarySerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.Serialization.XmlStd
{
    public static class Extensions
    {
        public static IDeaConnector UseBinarySerializer(this IDeaConnector deaConnetor)
        {
            var instance = deaConnetor.UseSerializer(() => new BinaryFormatterSerializer());
            return instance;
        }

        public static IDeaProcessor UseBinarySerializer(this IDeaProcessor deaProcessor)
        {
            var instance = deaProcessor.UseSerializer(() => new BinaryFormatterSerializer());
            return instance;
        }
    }
}

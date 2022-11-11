using DEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.Serialization.NewtonsoftJson
{
    public static class Extensions
    {
        public static IDeaConnector UseNewtonsoftJsonSerializer(this IDeaConnector deaConnetor)
        {
            var instance = deaConnetor.UseSerializer(() => new NewtonsoftJsonSerializer());
            return instance;
        }

        public static IDeaProcessor UseNewtonsoftJsonSerializer(this IDeaProcessor deaProcessor)
        {
            var instance = deaProcessor.UseSerializer(() => new NewtonsoftJsonSerializer());
            return instance;
        }
    }
}

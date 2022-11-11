using DEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.Compression.LZ4
{
    public static class Extensions
    {
        public static IDeaConnector UseLz4Compression(this IDeaConnector deaConnetor)
        {
            var instance = deaConnetor.UseCompression(() => new Lz4Compressor());
            return instance;
        }

        public static IDeaProcessor UseLz4Compression(this IDeaProcessor deaProcessor)
        {
            var instance = deaProcessor.UseCompression(() => new Lz4Compressor());
            return instance;
        }
    }
}

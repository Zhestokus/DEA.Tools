using DEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.Compression.Deflate
{
    public static class Extensions
    {
        public static IDeaConnector UseDeflateCompression(this IDeaConnector deaConnetor)
        {
            var instance = deaConnetor.UseCompression(() => new DeflateCompressor());
            return instance;
        }

        public static IDeaProcessor UseDeflateCompression(this IDeaProcessor deaProcessor)
        {
            var instance = deaProcessor.UseCompression(() => new DeflateCompressor());
            return instance;
        }
    }
}

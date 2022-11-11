using DEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.Compression.Gzip
{
    public static class Extensions
    {
        public static IDeaConnector UseGZipCompression(this IDeaConnector deaConnetor)
        {
            var instance = deaConnetor.UseCompression(() => new GZipCompressor());
            return instance;
        }

        public static IDeaProcessor UseGZipCompression(this IDeaProcessor deaProcessor)
        {
            var instance = deaProcessor.UseCompression(() => new GZipCompressor());
            return instance;
        }
    }
}

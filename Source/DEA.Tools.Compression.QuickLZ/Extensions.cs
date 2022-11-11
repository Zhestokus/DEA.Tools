using DEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.Compression.QuickLZ
{
    public static class Extensions
    {
        public static IDeaConnector UseQuickLzCompression(this IDeaConnector deaConnetor)
        {
            var instance = deaConnetor.UseCompression(() => new QuickLzCompressor());
            return instance;
        }

        public static IDeaProcessor UseQuickLzCompression(this IDeaProcessor deaProcessor)
        {
            var instance = deaProcessor.UseCompression(() => new QuickLzCompressor());
            return instance;
        }
    }
}

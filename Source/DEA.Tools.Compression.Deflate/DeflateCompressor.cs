using DEA.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.Compression.Deflate
{
    public class DeflateCompressor : DataCompressorBase
    {
        public override byte[] Compress(byte[] data)
        {
            using (var output = new MemoryStream())
            {
                using (var input = new MemoryStream(data))
                {
                    using (var deflate = new DeflateStream(output, CompressionMode.Compress, true))
                        input.CopyTo(deflate);
                }

                var result = output.ToArray();
                return result;
            }
        }

        public override byte[] Decompress(byte[] data)
        {
            using (var output = new MemoryStream())
            {
                using (var input = new MemoryStream(data))
                {
                    using (var deflate = new DeflateStream(input, CompressionMode.Decompress, true))
                        deflate.CopyTo(output);
                }

                var result = output.ToArray();
                return result;
            }
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}

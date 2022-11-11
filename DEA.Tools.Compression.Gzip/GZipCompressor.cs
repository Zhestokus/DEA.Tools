using DEA.Core;
using System;
using System.IO;
using System.IO.Compression;

namespace DEA.Tools.Compression.Gzip
{
    public class GZipCompressor : DataCompressorBase
    {
        public override byte[] Compress(byte[] data)
        {
            using (var output = new MemoryStream())
            {
                using (var input = new MemoryStream(data))
                {
                    using (var compressor = new GZipStream(output, CompressionMode.Compress, true))
                        input.CopyTo(compressor);
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
                    using (var compressor = new GZipStream(input, CompressionMode.Decompress, true))
                        compressor.CopyTo(output);
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

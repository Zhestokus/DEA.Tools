using DEA.Core;
using System;
using System.Runtime.InteropServices;

namespace DEA.Tools.Compression.QuickLZ
{
    public class QuickLzCompressor : DataCompressorBase
    {
        private const int QLZ_DEFAULT_LEVEL = 1;


        [DllImport("quicklz150_32_1.dll", EntryPoint = "qlz_compress")]
        private static extern int Compress_x86(byte[] source, byte[] destination, int size, byte[] scratch);

        [DllImport("quicklz150_32_1.dll", EntryPoint = "qlz_decompress")]
        private static extern int Decompress_x86(byte[] source, byte[] destination, byte[] scratch);

        [DllImport("quicklz150_32_1.dll", EntryPoint = "qlz_size_compressed")]
        private static extern int CompressedSize_x86(byte[] source);

        [DllImport("quicklz150_32_1.dll", EntryPoint = "qlz_size_decompressed")]
        private static extern int DecompressedSize_x86(byte[] source);

        [DllImport("quicklz150_32_1.dll", EntryPoint = "qlz_get_setting")]
        private static extern int GetSetting_x86(int setting);


        [DllImport("quicklz150_64_1.dll", EntryPoint = "qlz_compress")]
        private static extern int Compress_x64(byte[] source, byte[] destination, int size, byte[] scratch);

        [DllImport("quicklz150_64_1.dll", EntryPoint = "qlz_decompress")]
        private static extern int Decompress_x64(byte[] source, byte[] destination, byte[] scratch);

        [DllImport("quicklz150_64_1.dll", EntryPoint = "qlz_size_compressed")]
        private static extern int CompressedSize_x64(byte[] source);

        [DllImport("quicklz150_64_1.dll", EntryPoint = "qlz_size_decompressed")]
        private static extern int DecompressedSize_x64(byte[] source);

        [DllImport("quicklz150_64_1.dll", EntryPoint = "qlz_get_setting")]
        private static extern int GetSetting_x64(int setting);



        private readonly uint QLZ_COMPRESSION_LEVEL;
        private readonly uint QLZ_SCRATCH_COMPRESS;
        private readonly uint QLZ_SCRATCH_DECOMPRESS;
        private readonly uint QLZ_VERSION_MAJOR;
        private readonly uint QLZ_VERSION_MINOR;
        private readonly int QLZ_VERSION_REVISION;
        private readonly uint QLZ_STREAMING_BUFFER;
        private readonly bool QLZ_MEMORY_SAFE;

        private readonly bool _is64BitProcess;

        public QuickLzCompressor()
        {
            _is64BitProcess = Environment.Is64BitProcess;

            if (_is64BitProcess)
            {
                QLZ_COMPRESSION_LEVEL = (uint)GetSetting_x64(0);
                QLZ_SCRATCH_COMPRESS = (uint)GetSetting_x64(1);
                QLZ_SCRATCH_DECOMPRESS = (uint)GetSetting_x64(2);
                QLZ_VERSION_MAJOR = (uint)GetSetting_x64(7);
                QLZ_VERSION_MINOR = (uint)GetSetting_x64(8);
                QLZ_VERSION_REVISION = GetSetting_x64(9);
                QLZ_STREAMING_BUFFER = (uint)GetSetting_x64(3);
                QLZ_MEMORY_SAFE = GetSetting_x64(6) == 1;

                if (QLZ_STREAMING_BUFFER != 0U)
                    return;

                QLZ_SCRATCH_DECOMPRESS = QLZ_SCRATCH_COMPRESS;
            }
            else
            {
                QLZ_COMPRESSION_LEVEL = (uint)GetSetting_x86(0);
                QLZ_SCRATCH_COMPRESS = (uint)GetSetting_x86(1);
                QLZ_SCRATCH_DECOMPRESS = (uint)GetSetting_x86(2);
                QLZ_VERSION_MAJOR = (uint)GetSetting_x86(7);
                QLZ_VERSION_MINOR = (uint)GetSetting_x86(8);
                QLZ_VERSION_REVISION = GetSetting_x86(9);
                QLZ_STREAMING_BUFFER = (uint)GetSetting_x86(3);
                QLZ_MEMORY_SAFE = GetSetting_x86(6) == 1;

                if (QLZ_STREAMING_BUFFER != 0U)
                    return;

                QLZ_SCRATCH_DECOMPRESS = QLZ_SCRATCH_COMPRESS;
            }

        }

        public override byte[] Compress(byte[] data)
        {
            return CompressImpl(data);
        }

        public override byte[] Decompress(byte[] data)
        {
            return DecompressImpl(data);
        }

        private uint CompressedSize(byte[] source)
        {
            if (_is64BitProcess)
                return (uint)CompressedSize_x64(source);

            return (uint)CompressedSize_x86(source);
        }

        private uint DecompressedSize(byte[] source)
        {
            if (_is64BitProcess)
                return (uint)DecompressedSize_x64(source);

            return (uint)DecompressedSize_x86(source);
        }

        private byte[] CompressImpl(byte[] source) => CompressImpl(source, QLZ_DEFAULT_LEVEL);

        private byte[] CompressImpl(byte[] source, int level)
        {
            var length = source.Length;
            var scratch = new byte[QLZ_SCRATCH_COMPRESS];
            var buffer = new byte[length + 400];

            var size = 0;
            if (_is64BitProcess)
                size = Compress_x64(source, buffer, length, scratch);
            else
                size = Compress_x86(source, buffer, length, scratch);

            var compressed = new byte[size];
            Buffer.BlockCopy(buffer, 0, compressed, 0, size);

            return compressed;
        }

        private byte[] DecompressImpl(byte[] source)
        {
            var length = DecompressedSize_x86(source);
            var scratch = new byte[QLZ_SCRATCH_DECOMPRESS];

            var decompressed = new byte[length];

            var size = 0;
            if (_is64BitProcess)
                size = Decompress_x64(source, decompressed, scratch);
            else
                size = Decompress_x86(source, decompressed, scratch);

            if (size < length)
            {
                var buffer = new byte[size];
                Buffer.BlockCopy(decompressed, 0, buffer, 0, size);

                decompressed = buffer;
            }

            return decompressed;
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}

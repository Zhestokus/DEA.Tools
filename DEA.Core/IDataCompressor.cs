using System;

namespace DEA.Core
{
    public interface IDataCompressor : IDisposable
    {
        byte[] Compress(byte[] data);
        byte[] Decompress(byte[] data);
    }
}

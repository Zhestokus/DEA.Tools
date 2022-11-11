using System;
using System.IO;

namespace DEA.Core
{
    public interface IDataSerializer : IDisposable
    {
        void Serialize(Stream output, object obj);
        object Deserailize(Stream input, Type type);
    }
}

using DEA.Core;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DEA.Tools.Serialization.BinarySerializer
{
    public class BinaryFormatterSerializer : DataSerializerBase
    {
        public override object Deserailize(Stream input, Type type)
        {
            var serializer = new BinaryFormatter();
            var @object = serializer.Deserialize(input);

            return @object;
        }

        public override void Serialize(Stream output, object obj)
        {
            if (obj == null)
                return;

            var serializer = new BinaryFormatter();
            serializer.Serialize(output, obj);
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}

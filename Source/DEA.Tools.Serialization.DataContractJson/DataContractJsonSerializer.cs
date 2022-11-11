using DEA.Core;
using System;
using System.IO;

namespace DEA.Tools.Serialization.DataContractJson
{
    public class DataContractJsonSerializer : DataSerializerBase
    {
        public override object Deserailize(Stream input, Type type)
        {
            var serializer = new DataContractJsonSerializer();
            var @object = serializer.Deserailize(input, type);

            return @object;
        }

        public override void Serialize(Stream output, object obj)
        {
            if (obj == null)
                return;

            var serializer = new DataContractJsonSerializer();
            serializer.Serialize(output, obj);
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}

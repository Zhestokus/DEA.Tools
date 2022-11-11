using DEA.Core;
using System;
using System.IO;
using System.Xml.Serialization;

namespace DEA.Tools.Serialization.XmlStd
{
    public class StandardXmlSerializer : DataSerializerBase
    {
        public override object Deserailize(Stream input, Type type)
        {
            var serializer = new XmlSerializer(type);
            var @object = serializer.Deserialize(input);

            return @object;
        }

        public override void Serialize(Stream output, object obj)
        {
            if (obj == null)
                return;

            var serializer = new XmlSerializer(obj.GetType());
            serializer.Serialize(output, obj);
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}

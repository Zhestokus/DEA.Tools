using DEA.Core;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace DEA.Tools.Serialization.DataContractXml
{
    public class DataContractXmlSerializer : DataSerializerBase
    {
        public override object Deserailize(Stream input, Type type)
        {
            var reader = new XmlTextReader(input);

            var serializer = new DataContractSerializer(type);
            var @object = serializer.ReadObject(reader);

            return @object;
        }

        public override void Serialize(Stream output, object obj)
        {
            if (obj == null)
                return;

            var writer = new XmlTextWriter(output, Encoding.UTF8);
            var type = obj.GetType();

            var serializer = new DataContractSerializer(type);
            serializer.WriteObject(writer, obj);
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}

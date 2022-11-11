using DEA.Core;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace DEA.Tools.Serialization.NewtonsoftJson
{
    public class NewtonsoftJsonSerializer : DataSerializerBase
    {
        private readonly JsonSerializer _serilizer;

        public NewtonsoftJsonSerializer()
        {
            _serilizer = new JsonSerializer();
        }

        public override object Deserailize(Stream input, Type type)
        {
            using (var reader = new StreamReader(input, Encoding.UTF8, false, 4096, true))
            {
                var @object = _serilizer.Deserialize(reader, type);
                return @object;
            }
        }

        public override void Serialize(Stream output, object obj)
        {
            if (obj == null)
                return;

            using (var writer = new StreamWriter(output, Encoding.UTF8, 4096, true))
            {
                var type = obj.GetType();

                _serilizer.Serialize(writer, obj, type);
            }
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}

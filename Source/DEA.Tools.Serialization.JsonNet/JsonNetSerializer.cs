using DEA.Core;
using System;
using System.IO;
using System.Text.Json;

namespace DEA.Tools.Serialization.JsonNet
{
    public class JsonNetSerializer : DataSerializerBase
    {
        public override object Deserailize(Stream input, Type type)
        {
            var @object = JsonSerializer.Deserialize(input, type);
            return @object;
        }

        public override void Serialize(Stream output, object obj)
        {
            if (obj == null)
                return;

            var type = obj.GetType();

            JsonSerializer.Serialize(output, type);
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}

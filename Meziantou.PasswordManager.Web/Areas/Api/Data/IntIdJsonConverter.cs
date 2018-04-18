using System;
using Newtonsoft.Json;

namespace Meziantou.PasswordManager.Web.Areas.Api.Data
{
    public class IntIdJsonConverter : JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IntId);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            switch (value)
            {
                case long l:
                    return new IntId((int)l);

                case int i:
                    return new IntId(i);

                default:
                    return new IntId();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var v = (IntId)value;
            if (v.HasValue)
            {
                writer.WriteValue(v.Value);
            }
            else
            {
                writer.WriteValue(-1);
            }
        }
    }
}

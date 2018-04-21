using System;
using Newtonsoft.Json;

namespace Meziantou.PasswordManager.Web.Areas.Api.Data
{
    internal class UserRefConverter : JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(UserRef);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            switch (value)
            {
                case string s:
                    return new UserRef(s);

                default:
                    return new UserRef();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var r = (UserRef)value;
            writer.WriteValue(r.Email);
        }
    }
}
